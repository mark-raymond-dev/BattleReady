using BattleReady.Core.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

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
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                    var exception = exceptionFeature?.Error;

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

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                // Original endpoint — unhandled generic exception → 500
                endpoints.MapGet("/test/throw", context =>
                {
                    throw new InvalidOperationException("Deliberate test exception.");
                });

                // Domain exception endpoints for testing the new mappings
                endpoints.MapGet("/test/not-found", context =>
                {
                    throw new NotFoundException("The requested resource was not found.");
                });

                endpoints.MapGet("/test/validation", context =>
                {
                    throw new ValidationException("The request violates a domain rule.");
                });

                endpoints.MapGet("/test/domain", context =>
                {
                    throw new TestDomainException("A generic domain error occurred.");
                });

                endpoints.MapControllers();
            });
        });
    }
}

// Concrete subclass of DomainException for testing the base-class catch.
// We can't instantiate DomainException directly because it's abstract.
file class TestDomainException : DomainException
{
    public TestDomainException(string message) : base(message) { }
}