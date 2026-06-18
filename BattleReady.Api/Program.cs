using BattleReady.Core.Features.Calculator.Services;
using BattleReady.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Formatting.Compact;
using Asp.Versioning;

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

// Register your services
builder.Services.AddScoped<ICalculationService, CalculationService>();
builder.Services.AddScoped<IHitChanceService, HitChanceService>();
builder.Services.AddScoped<IParseDamageService, ParseDamageService>();

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

var app = builder.Build();

app.UseExceptionHandler();  // used in conjunction with: builder.Services.AddProblemDetails();

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

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

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