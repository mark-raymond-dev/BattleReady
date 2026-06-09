using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;
using BattleReady.Api.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using BattleReady.Api.Mapping;

namespace BattleReady.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalculatorController : ControllerBase
{
    private readonly CalculationService _service;

    public CalculatorController(CalculationService service)
    {
        _service = service;
    }

    [HttpPost("calculate")]
    public ActionResult<CalculationResponse> Calculate([FromBody] CalculationRequest request)
    {
        // Maps API request models to Core input models via extension methods in BattleReady.Api/Mapping/

        var response = _service.Calculate(request.ToInput());
        return Ok(response);        
    }
}