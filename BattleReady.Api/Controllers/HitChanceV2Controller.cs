using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;
using BattleReady.Api.Models.Requests;
using BattleReady.Api.Filters;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;

namespace BattleReady.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/hitchance")]
public class HitChanceV2Controller : ControllerBase
{
    private readonly IHitChanceService _service;
    private readonly IMemoryCache _cache;

    public HitChanceV2Controller(IHitChanceService service, IMemoryCache cache)
    {
        _service = service;
        _cache   = cache;
    }

    [HttpPost("calculate")]
    [Authorize]
    [ServiceFilter(typeof(RequestLoggingFilter))]
    public ActionResult<HitChanceV2Response> Calculate([FromBody] HitChanceRequest request)
    {
        var result = _service.Calculate(
            request.ToHit    ?? 0,
            request.Defense  ?? 0,
            request.Natural20Upgrades,
            request.Natural1Downgrades);

        return Ok(new HitChanceV2Response
        {
            CritMissChance   = result.CritMissChance,
            NormalMissChance = result.NormalMissChance,
            NormalHitChance  = result.NormalHitChance,
            CritHitChance    = result.CritHitChance,
        });
    }

    [HttpGet("calculate")]
    [ResponseCache(Duration = 60)]
    public ActionResult<HitChanceV2Response> Get([FromQuery] HitChanceRequest request)
    {
        var cacheKey = $"HitChanceV2:{request.ToHit}:{request.Defense}:{request.Natural20Upgrades}:{request.Natural1Downgrades}";

        var response = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
            var result = _service.Calculate(
                request.ToHit   ?? 0,
                request.Defense ?? 0,
                request.Natural20Upgrades,
                request.Natural1Downgrades);

            return new HitChanceV2Response
            {
                CritMissChance   = result.CritMissChance,
                NormalMissChance = result.NormalMissChance,
                NormalHitChance  = result.NormalHitChance,
                CritHitChance    = result.CritHitChance,
            };
        });

        return Ok(response);
    }
}
