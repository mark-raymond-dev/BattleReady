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
    private readonly CalculateService _service;

    public CalculatorController(CalculateService service)
    {
        _service = service;
    }

    [HttpPost("calculate")]
    public ActionResult<CalculationResponse> Calculate([FromBody] CalculationRequest request)
    {
        // IMPORTANT NOTE:  Unpacking properties at the controller boundary like this is the preferred pattern.
        // The controller is the natural translation layer between HTTP concerns and domain concerns.
        
        // We are mapping CalculationRequest (API request model) to CalculationInput (domain-layer).
        // Similiarly, we are mapping AttackRequest (API) to AttackInput (Core).
        // By doing this, we decouple Core from the API.

        var response = _service.Calculate(request.ToInput());
        return Ok(response);        
    }
}