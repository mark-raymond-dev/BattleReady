using BattleReady.Core.Features.Calculator.Services;
using BattleReady.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

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

// Uncomment this line for a throw-away endpoint you can hit to see the difference between having ProblemDetails and not having it.
//app.MapGet("/throw", () => { throw new InvalidOperationException("This is a deliberate test exception for verifying Problem Details formatting."); });

app.Run();

// This is a C# trick — it exposes Program as a
// public partial class so the test project can reference it.
// Without this line, WebApplicationFactory<Program> won't compile.
public partial class Program { }