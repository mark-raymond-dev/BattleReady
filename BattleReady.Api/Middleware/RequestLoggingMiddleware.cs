using Serilog.Context;

namespace BattleReady.Api.Middleware;

// Middleware that pushes the current request's ID into Serilog's LogContext
// for the duration of the request. This causes every log line written anywhere
// during the request — controllers, services, EF Core, filters — to automatically
// include a CorrelationId property, making all log lines for a single request
// greppable as a unit.
//
// HttpContext.TraceIdentifier is used as the correlation ID because it's the same
// value ASP.NET Core's own built-in log lines use for their RequestId field,
// so custom and framework log lines share the same identifier naturally.
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // LogContext.PushProperty returns an IDisposable. Disposing it removes
        // the property from the context — ensuring it doesn't leak into log lines
        // from subsequent unrelated requests.
        using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
        {
            await _next(context);
        }
    }
}