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
public class ParseDamageController : ControllerBase
{
    private readonly IParseDamageService _service;
    private readonly AppDbContext _db;

    public ParseDamageController(IParseDamageService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<ParseDamageResponse>> Calculate([FromBody] ParseDamageRequest request)
    {
        // IMPORTANT NOTE:  Unpacking properties at the controller boundary like this is the preferred pattern.
        // The controller is the natural translation layer between HTTP concerns and domain concerns.

        // We don't need to map the request object to a corresponding input object because
        // the service doesn't take an object - it simply accepts a series of primitives.

        var response = _service.Calculate(
            request.Expression
            );

        await _db.ApiRequestLogs.AddAsync(new ApiRequestLog
        {
            Endpoint = "POST /api/ParseDamage/calculate",
            RequestBody = JsonSerializer.Serialize(request),
            ResponseBody = JsonSerializer.Serialize(response),
            ResponseStatus = 200
        });
        await _db.SaveChangesAsync();

        return Ok(response);
    }

    [HttpGet("calculate")]
    [ResponseCache(Duration = 60)]
    public ActionResult<ParseDamageResponse> Get([FromQuery] ParseDamageRequest request)
    {
        var response = _service.Calculate(
            request.Expression
            );

        return Ok(response);
    }
}