using System.Net;
using System.Net.Http.Json;
using BattleReady.Api.Models.Requests;
using BattleReady.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BattleReady.Tests.Integration;

public class ParseDamageControllerTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFactory _factory;

    public ParseDamageControllerTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Calculate_ValidRequest_Returns200()
    {
        var request = new ParseDamageRequest { Expression = "2d6+3 slashing" };

        var response = await _client.PostAsJsonAsync("/api/ParseDamage/calculate", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Calculate_ValidRequest_LogsRequestToDatabase()
    {
        var request = new ParseDamageRequest { Expression = "2d6+3 slashing" };

        await _client.PostAsJsonAsync("/api/ParseDamage/calculate", request);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var log = db.ApiRequestLogs.FirstOrDefault(x => x.Endpoint == "POST /api/ParseDamage/calculate");

        Assert.NotNull(log);
        Assert.Equal(200, log.ResponseStatus);
    }

    [Fact]
    public async Task Calculate_MissingRequiredFields_Returns400()
    {
        var request = new { };

        var response = await _client.PostAsJsonAsync("/api/ParseDamage/calculate", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}