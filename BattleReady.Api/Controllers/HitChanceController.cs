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
    public ActionResult<HitChanceResponse> Calculate([FromBody] HitChanceRequest request)
    {
        // IMPORTANT NOTE:  Unpacking properties at the controller boundary like this is the preferred pattern.
        // The controller is the natural translation layer between HTTP concerns and domain concerns.

        var response = _service.Calculate(
            request.ToHit ?? 0, 
            request.Defense,
            request.Natural20Upgrades, 
            request.Natural1Downgrades
            );
        return Ok(response);
    }
}