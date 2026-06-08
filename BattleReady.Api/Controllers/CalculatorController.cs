using BattleReady.Features.Calculator.Models;
using BattleReady.Features.Calculator.Services;
using Microsoft.AspNetCore.Mvc;

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
        // By doing this, we decouple Core from the API.
        
        var input = new CalculationInput
        {
            CharacterName = request.CharacterName,
            EnemyDefense = request.EnemyDefense,
            Attacks = request.Attacks,
            DefaultAttack = request.DefaultAttack,
            Notes = request.Notes,
            Natural20Upgrades = request.Natural20Upgrades,
            Natural1Downgrades = request.Natural1Downgrades
        };

        var response = _service.Calculate(input);
        return Ok(response);        
    }
}