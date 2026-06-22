using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace BattleReady.Tests.Integration;

public class ExceptionHandlerTestFactory : IntegrationTestFactory
{
    public ExceptionHandlerTestFactory() : base("ExceptionHandlerTestDb") { }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // This sets up the database. Service registrations are preserved even
        // though builder.Configure below replaces the middleware pipeline.
        base.ConfigureWebHost(builder);

        // This replaces the middleware pipeline, which is fine here because we only need the
        // essential pieces: exception handling, routing, and controller mapping. The real API
        // controllers are still discovered by MapControllers() since their service registrations 
        // from Program.cs were preserved above. We add our throw endpoint alongside them via 
        // MapGet — no controller class needed, just a route and a delegate.
        builder.Configure(app =>
        {
            app.UseExceptionHandler();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/test/throw", context =>
                {
                    throw new InvalidOperationException("Deliberate test exception.");
                });

                endpoints.MapControllers();
            });
        });
    }
}