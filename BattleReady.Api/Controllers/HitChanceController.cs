using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;
using BattleReady.Api.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using BattleReady.Data;
using BattleReady.Data.Entities;
using System.Text.Json;

namespace BattleReady.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HitChanceController : ControllerBase
{
    private readonly HitChanceService _service;
    private readonly AppDbContext _db;

    public HitChanceController(HitChanceService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<HitChanceResponse>> Calculate([FromBody] HitChanceRequest request)
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
            Endpoint = "POST /api/HitChance/calculate",
            RequestBody = JsonSerializer.Serialize(request),
            ResponseStatus = 200
        });
        await _db.SaveChangesAsync();

        return Ok(response);
    }
}