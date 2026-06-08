using BattleReady.Features.Calculator.Models;
using BattleReady.Features.Calculator.Services;
using Microsoft.AspNetCore.Mvc;

namespace BattleReady.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HitChanceController : ControllerBase
{
    private readonly HitChanceService _service;

    public HitChanceController(HitChanceService service)
    {
        _service = service;
    }

    [HttpPost("calculate")]
    public ActionResult<HitChance> Calculate([FromBody] HitChanceRequest request)
    {
        var result = _service.Calculate(request.ToHit, request.Defense,
                                        request.Natural20Upgrades, request.Natural1Downgrades);
        return Ok(result);
    }
}