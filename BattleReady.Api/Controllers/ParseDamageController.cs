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
    public async Task<ActionResult<ParseDamageResponse>> Calculate(
        [FromBody] ParseDamageRequest request,
        CancellationToken cancellationToken)
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
            Endpoint = "POST /api/v1/ParseDamage/calculate",     // be sure to set the version correctly here
            RequestBody = JsonSerializer.Serialize(request),
            ResponseBody = JsonSerializer.Serialize(response),
            ResponseStatus = 200
        }, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

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