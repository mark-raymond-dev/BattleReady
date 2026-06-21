using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;
using BattleReady.Api.Models.Requests;
using BattleReady.Api.Mapping;
using BattleReady.Api.Filters;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace BattleReady.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CalculatorController : ControllerBase
{
    private readonly ICalculationService _service;
    
    public CalculatorController(ICalculationService service)
    {
        _service = service;
    }

    [HttpPost("calculate")]
    [ServiceFilter(typeof(RequestLoggingFilter))] // this COULD go at the class level, but for consistency we put it here at the method
    public ActionResult<CalculationResponse> Calculate([FromBody] CalculationRequest request)
    {
        // Maps API request models to Core input models via extension methods in BattleReady.Api/Mapping/
        var response = _service.Calculate(request.ToInput());
        return Ok(response);        
    }
}