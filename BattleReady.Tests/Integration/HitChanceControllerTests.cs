using System.Net;
using System.Net.Http.Json;
using BattleReady.Api.Models.Requests;
using BattleReady.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BattleReady.Tests.Integration;

public class HitChanceControllerTests : IClassFixture<HitChanceTestFactory>
{
    private readonly HttpClient _client;
    private readonly HitChanceTestFactory _factory;

    public HitChanceControllerTests(HitChanceTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Calculate_ValidRequest_Returns200()
    {
        var request = new HitChanceRequest
        {
            ToHit = 12,
            Defense = 19,
            Natural20Upgrades = true,
            Natural1Downgrades = true
        };

        var response = await _client.PostAsJsonAsync("/api/HitChance/calculate", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Calculate_ValidRequest_LogsRequestToDatabase()
    {
        var request = new HitChanceRequest
        {
            ToHit = 12,
            Defense = 19,
            Natural20Upgrades = true,
            Natural1Downgrades = true
        };

        await _client.PostAsJsonAsync("/api/HitChance/calculate", request);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var log = db.ApiRequestLogs.FirstOrDefault(x => x.Endpoint == "POST /api/HitChance/calculate");

        Assert.NotNull(log);
        Assert.Equal(200, log.ResponseStatus);
        Assert.NotEmpty(log.ResponseBody);
    }

    [Fact]
    public async Task Calculate_MissingRequiredFields_Returns400()
    {
        var request = new { };

        var response = await _client.PostAsJsonAsync("/api/HitChance/calculate", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}