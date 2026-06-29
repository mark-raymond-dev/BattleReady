using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace BattleReady.Tests.Integration;

public class AuthControllerTests : IClassFixture<AuthTestFactory>
{
    private const string Version     = "v1";
    private const string TokenUrl    = $"/api/{Version}/Auth/token";
    private const string ProtectedUrl = "/api/v2/hitchance/calculate";

    private readonly HttpClient _client;

    public AuthControllerTests(AuthTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    // -----------------------------------------------------------------------
    // Token endpoint
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetToken_ValidCredentials_Returns200WithToken()
    {
        var response = await _client.PostAsJsonAsync(TokenUrl, new
        {
            username = "battleready",
            password = "password123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;

        // Response must contain a "token" property with a non-empty value
        Assert.True(json.TryGetProperty("token", out var tokenProp));
        Assert.False(string.IsNullOrWhiteSpace(tokenProp.GetString()));
    }

    [Fact]
    public async Task GetToken_InvalidCredentials_Returns401()
    {
        var response = await _client.PostAsJsonAsync(TokenUrl, new
        {
            username = "wrong",
            password = "wrong"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -----------------------------------------------------------------------
    // Protected endpoint
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ProtectedEndpoint_NoToken_Returns401()
    {
        var request = new { toHit = 12, defense = 19, natural20Upgrades = true, natural1Downgrades = true };

        var response = await _client.PostAsJsonAsync(ProtectedUrl, request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_ValidToken_Returns200()
    {
        // First get a token
        var tokenResponse = await _client.PostAsJsonAsync(TokenUrl, new
        {
            username = "battleready",
            password = "password123"
        });

        var tokenBody = await tokenResponse.Content.ReadAsStringAsync();
        var token = JsonDocument.Parse(tokenBody)
            .RootElement
            .GetProperty("token")
            .GetString();

        // Then call the protected endpoint with the token
        var request = new HttpRequestMessage(HttpMethod.Post, ProtectedUrl)
        {
            Content = JsonContent.Create(new
            {
                toHit              = 12,
                defense            = 19,
                natural20Upgrades  = true,
                natural1Downgrades = true
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}