using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;

namespace BattleReady.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("token")]
    public ActionResult<string> GetToken([FromBody] LoginRequest request)
    {
        // In a real system, you would validate credentials against a database.
        // For this portfolio project, we use a hardcoded test user to keep the
        // focus on the JWT mechanics rather than user management infrastructure.
        if (request.Username != "battleready" || request.Password != "password123")
            return Unauthorized("Invalid credentials.");

        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry  = DateTime.UtcNow.AddMinutes(
                          double.Parse(_configuration["Jwt:ExpiryMinutes"]!));

        // Claims are key/value pairs embedded in the token.
        // The consumer can read them without contacting the server.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,  request.Username),
            new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:             _configuration["Jwt:Issuer"],
            audience:           _configuration["Jwt:Audience"],
            claims:             claims,
            expires:            expiry,
            signingCredentials: creds);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}

// Request model — lives here since it's only used by this controller.
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}