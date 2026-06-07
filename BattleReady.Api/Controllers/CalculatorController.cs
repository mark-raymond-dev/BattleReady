using BattleReady.Features.Calculator.Models;
using BattleReady.Features.Calculator.Services;
using Microsoft.AspNetCore.Mvc;

namespace BattleReady.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalculatorController : ControllerBase
{
    private readonly CalculateResponseService _service;

    public CalculatorController(CalculateResponseService service)
    {
        _service = service;
    }

    [HttpPost("calculate")]
    public ActionResult<CalculationResponse> Calculate([FromBody] CalculationRequest request)
    {
        var response = _service.Calculate(request);
        return Ok(response);
    }
}