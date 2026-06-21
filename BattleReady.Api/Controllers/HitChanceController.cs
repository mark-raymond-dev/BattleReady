using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;
using BattleReady.Api.Models.Requests;
using BattleReady.Api.Filters;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace BattleReady.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HitChanceController : ControllerBase
{
    private readonly IHitChanceService _service;

    public HitChanceController(IHitChanceService service)
    {
        _service = service;
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
        var response = _service.Calculate(
            request.ToHit ?? 0,
            request.Defense ?? 0,
            request.Natural20Upgrades,
            request.Natural1Downgrades
            );

        return Ok(response);
    }
}