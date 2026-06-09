using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;
using BattleReady.Api.Models.Requests;
using BattleReady.Api.Mapping;
using BattleReady.Data;
using BattleReady.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BattleReady.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalculatorController : ControllerBase
{
    private readonly CalculationService _service;
    private readonly AppDbContext _db;

    public CalculatorController(CalculationService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<CalculationResponse>> Calculate([FromBody] CalculationRequest request)
    {
        // Maps API request models to Core input models via extension methods in BattleReady.Api/Mapping/

        var response = _service.Calculate(request.ToInput());

        await _db.ApiRequestLogs.AddAsync(new ApiRequestLog
        {
            Endpoint = "POST /api/Calculator/calculate",
            RequestBody = JsonSerializer.Serialize(request),
            ResponseStatus = 200
        });
        await _db.SaveChangesAsync();

        return Ok(response);        
    }
}