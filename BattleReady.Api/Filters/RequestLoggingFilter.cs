using BattleReady.Data;
using BattleReady.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace BattleReady.Api.Filters;

// NOTE:  Action filters sit at a specific stage in request pipelines: after routing has resolved which
// controller and action will handle the request, but before the action method body actually executes.
// That positioning gives a filter access to the request (already routed, parameters already bound)
// and the response (available after the action runs).

// An action filter that runs after every decorated controller action and writes 
// a structured log entry to the database. Using a filter rather than repeating
// the logging block in each controller keeps this cross-cutting concern in one
// place, and derives the endpoint string from the live request rather than a
// hardcoded literal — so a route rename or version bump never silently diverges.
public class RequestLoggingFilter : IAsyncActionFilter
{
    private readonly AppDbContext _db;

    public RequestLoggingFilter(AppDbContext db)
    {
        _db = db;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,         // read-only window into the request
        ActionExecutionDelegate next            // allows you to execute the action itself
        )
    {
        // Capture the request body before the action runs.
        // context.ActionArguments contains the deserialized action parameters by name.
        // We serialize the whole dictionary so every parameter is captured regardless
        // of how many the action has — no need to know the parameter name in advance.
        var requestBody = JsonSerializer.Serialize(context.ActionArguments);

        // Execute the action. The returned context contains the result.
        var executedContext = await next();

        // Only log if the action completed without an unhandled exception,
        // and only if it returned an OkObjectResult (status 200).
        // We don't log validation failures (400s) or other non-success results,
        // consistent with the existing per-controller behavior.
        if (executedContext.Exception == null
            && executedContext.Result is OkObjectResult okResult)
        {
            var method = context.HttpContext.Request.Method;
            var path   = context.HttpContext.Request.Path.Value ?? string.Empty;

            await _db.ApiRequestLogs.AddAsync(new ApiRequestLog
            {
                Endpoint     = $"{method} {path}",
                RequestBody  = requestBody,
                ResponseBody = JsonSerializer.Serialize(okResult.Value),
                ResponseStatus = 200
            });

            await _db.SaveChangesAsync();
        }
    }
}