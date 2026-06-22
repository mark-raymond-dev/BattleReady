using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;
using BattleReady.Api.Models.Requests;
using BattleReady.Api.Filters;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.Extensions.Caching.Memory;

namespace BattleReady.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HitChanceController : ControllerBase
{
    private readonly IHitChanceService _service;
    private readonly IMemoryCache _cache;

    public HitChanceController(IHitChanceService service, IMemoryCache cache)
    {
        _service = service;
        _cache = cache;
    }

    [HttpPost("calculate")]
    [ServiceFilter(typeof(RequestLoggingFilter))] // we put this here instead of class level because we only want it to run for the POST method, not the GET method
    public ActionResult<HitChanceResponse> Calculate([FromBody] HitChanceRequest request)
    {
        // IMPORTANT NOTE:  Unpacking properties at the controller boundary like this is the preferred pattern.
        // The controller is the natural translation layer between HTTP concerns and domain concerns.

        // We don't need to map the request object to a corresponding input object because
        // the service doesn't take an object - it simply accepts a series of primitives.

        var response = _service.Calculate(
            request.ToHit ?? 0, 
            request.Defense ?? 0,
            request.Natural20Upgrades, 
            request.Natural1Downgrades
            );
        return Ok(response);
    }

    [HttpGet("calculate")]
    [ResponseCache(Duration = 60)]
    public ActionResult<HitChanceResponse> Get([FromQuery] HitChanceRequest request)
    {
        // Build a cache key from all inputs that affect the result.
        // If any input differs, it's a different calculation — different cache entry.
        var cacheKey = $"HitChance:{request.ToHit}:{request.Defense}:{request.Natural20Upgrades}:{request.Natural1Downgrades}";

        var response = _cache.GetOrCreate(cacheKey, entry =>
        {
            // Cache this result for 60 seconds — matches the [ResponseCache] hint
            // we're already sending to clients, so both layers expire at the same time.
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
            return _service.Calculate(
                request.ToHit ?? 0,
                request.Defense ?? 0,
                request.Natural20Upgrades,
                request.Natural1Downgrades
                );
        });

        return Ok(response);
    }
}