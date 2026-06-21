using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;
using BattleReady.Api.Models.Requests;
using BattleReady.Api.Filters;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace BattleReady.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ParseDamageController : ControllerBase
{
    private readonly IParseDamageService _service;

    public ParseDamageController(IParseDamageService service)
    {
        _service = service;
    }

    [HttpPost("calculate")]
    [ServiceFilter(typeof(RequestLoggingFilter))] // we put this here instead of class level because we only want it to run for the POST method, not the GET method
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