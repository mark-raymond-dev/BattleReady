using BattleReady.Features.Calculator.Models;
using BattleReady.Features.Calculator.Services;
using Microsoft.AspNetCore.Mvc;

namespace BattleReady.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParseDamageController : ControllerBase
{
    private readonly ParseDamageService _service;

    public ParseDamageController(ParseDamageService service)
    {
        _service = service;
    }

    [HttpPost("calculate")]
    public ActionResult<ParseDamageResponse> Calculate([FromBody] ParseDamageRequest request)
    {
        // IMPORTANT NOTE:  Unpacking properties at the controller boundary like this is the preferred pattern.
        // The controller is the natural translation layer between HTTP concerns and domain concerns.

        var response = _service.Calculate(
            request.Expression
            );
        return Ok(response);
    }
}