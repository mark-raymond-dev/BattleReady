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
    public ActionResult<ParseDamage> Calculate([FromBody] ParseDamageRequest request)
    {
        var result = _service.Calculate(request.Expression);
        return Ok(result);
    }
}