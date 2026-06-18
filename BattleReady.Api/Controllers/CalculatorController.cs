using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;
using BattleReady.Api.Models.Requests;
using BattleReady.Api.Mapping;
using Microsoft.AspNetCore.Mvc;
using BattleReady.Data;
using BattleReady.Data.Entities;
using System.Text.Json;
using Asp.Versioning;

namespace BattleReady.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CalculatorController : ControllerBase
{
    private readonly ICalculationService _service;
    private readonly AppDbContext _db;

    public CalculatorController(ICalculationService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<CalculationResponse>> Calculate(
        [FromBody] CalculationRequest request,
        CancellationToken cancellationToken)
    {
        // Maps API request models to Core input models via extension methods in BattleReady.Api/Mapping/

        var response = _service.Calculate(request.ToInput());

        await _db.ApiRequestLogs.AddAsync(new ApiRequestLog
        {
            Endpoint = "POST /api/v1/Calculator/calculate",     // be sure to set the version correctly here
            RequestBody = JsonSerializer.Serialize(request),
            ResponseBody = JsonSerializer.Serialize(response),
            ResponseStatus = 200
        }, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(response);        
    }
}