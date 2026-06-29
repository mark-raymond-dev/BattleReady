using BattleReady.Core.Features.Calculator.Services;
using BattleReady.Data;
using BattleReady.Api.Filters;
using BattleReady.Api.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Formatting.Compact;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using BattleReady.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Set up Serilog. The message levels are, in order:
// Verbose, Debug, Information, Warning, Error, Fatal.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()                                     // ignores Verbose or Debug messages
    .Enrich.FromLogContext()                                        // allows additional properties to be attached dynamically via LogContext.PushProperty(...)
    .WriteTo.Console(new CompactJsonFormatter())                    // SINK 1: writes logs to console
    .WriteTo.File(new CompactJsonFormatter(),                       // SINK 2: writes logs to file
        "logs/log-.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// If you comment out the Log.Logger ... lines above (e.g. to revert 
// back to the default logging), you must also comment out this line.
builder.Host.UseSerilog();

// Adds versioning to our Apis
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1.0);        // if no version is specified in a request, treat it as v1.
    options.AssumeDefaultVersionWhenUnspecified = true;     // conventional default - without this, an unversioned request would 400 outright
    options.ReportApiVersions = true;                       // adds api-supported-versions response header so consumers (and Swagger) can discover what versions exist without guessing
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Rate limiting — fixed window policy applied globally.
// Exemptions for /health and Swagger are handled at the middleware/route level below.
// Fixed window: each caller gets 30 requests per 10-second window.
// Excess requests receive 429 Too Many Requests.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.Window             = TimeSpan.FromSeconds(10);
        limiterOptions.PermitLimit        = 30;
        limiterOptions.QueueLimit         = 0;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// Register your services. This is essentially registering "recipes" with builder.Services. It's sort of 
// like saying "if anyone ever asks for an ICalculationService, here's how to make one: construct a 
// CalculationService." Nothing gets built yet at this point — you're just registering recipes.
builder.Services.AddScoped<ICalculationService, CalculationService>();
builder.Services.AddScoped<IHitChanceService, HitChanceService>();
builder.Services.AddScoped<IParseDamageService, ParseDamageService>();
builder.Services.AddScoped<ISpellSaveService, SpellSaveService>();
builder.Services.AddScoped<IDegreeOfSuccessService, DegreeOfSuccessService>();
builder.Services.AddScoped<RequestLoggingFilter>();

// Register the DbContext
// If a query fails mid-flight because the database went to sleep, EF Core 
// will retry it automatically up to 3 times before giving up.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null)));
    
    builder.Services.AddHealthChecks()
        .AddSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "sql-server",
            failureStatus: HealthStatus.Degraded);
}
else
{
    builder.Services.AddHealthChecks();
}

// This is the moment the recipe list gets turned into the actual container — a runtime object that hold
// all those registrations and is capable of resolving them, meaning: given a requested type, look up its
// recipe, construct it (recursively constructing anything it depends on too), and hand back the instance. 
var app = builder.Build();

// used in conjunction with: builder.Services.AddProblemDetails();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;

        // Translate domain exceptions to appropriate HTTP status codes, as
        // well as corresponding titles. Everything else falls through to 500.
        var statusCode = exception switch
        {
            NotFoundException       => StatusCodes.Status404NotFound,
            ValidationException     => StatusCodes.Status422UnprocessableEntity,
            DomainException         => StatusCodes.Status400BadRequest,
            _                       => StatusCodes.Status500InternalServerError
        };
        var title = exception switch
        {
            NotFoundException   => "Resource not found.",
            ValidationException => "Unprocessable entity.",
            DomainException     => "Bad request.",
            _                   => "An unexpected error occurred."
        };

        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/problem+json";

        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = statusCode,
            title  = title,
            detail = exception?.Message
        });

        await context.Response.WriteAsync(json);
    });
});

// Auto-run migrations on startup
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            // We put this in a try-catch as a safety precaution in case the database has not woken up yet.
            // This way if the database is slow to wake up, the app still starts successfully and Swagger is
            // accessible. The migration will run successfully on the next startup once both services are warm.
            db.Database.Migrate();    
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Migration failed on startup. The database may be waking up.");
        }        
    }    
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseRateLimiter();
app.UseSwagger();
app.UseSwaggerUI(); // Swagger registers routes before MapControllers() line below, therefore doesn't need explicit exemption
app.UseHttpsRedirection();
app.UseAuthentication(); // Must come before app.UseAuthorization()
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("fixed"); // applies fixed window policy to all controller endpoints
app.MapHealthChecks("/health").DisableRateLimiting(); // explicit exemption for the health endpoint

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

// This is a C# trick — it exposes Program as a
// public partial class so the test project can reference it.
// Without this line, WebApplicationFactory<Program> won't compile.
public partial class Program { }