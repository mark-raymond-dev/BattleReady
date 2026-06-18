using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;
using BattleReady.Api.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using BattleReady.Data;
using BattleReady.Data.Entities;
using System.Text.Json;
using Asp.Versioning;

namespace BattleReady.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HitChanceController : ControllerBase
{
    private readonly IHitChanceService _service;
    private readonly AppDbContext _db;

    public HitChanceController(IHitChanceService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<HitChanceResponse>> Calculate(
        [FromBody] HitChanceRequest request,
        CancellationToken cancellationToken)
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

        await _db.ApiRequestLogs.AddAsync(new ApiRequestLog
        {
            Endpoint = "POST /api/v1/HitChance/calculate",     // be sure to set the version correctly here
            RequestBody = JsonSerializer.Serialize(request),
            ResponseBody = JsonSerializer.Serialize(response),
            ResponseStatus = 200
        }, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

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