using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;
using BattleReady.Api.Models.Requests;
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

        // We don't need to map the request object to a corresponding input object because
        // the service doesn't take an object - it simply accepts a series of primitives.

        var response = _service.Calculate(
            request.Expression
            );
        return Ok(response);
    }
}