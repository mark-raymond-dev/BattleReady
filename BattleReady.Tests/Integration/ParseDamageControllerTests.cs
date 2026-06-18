using System.Net;
using System.Net.Http.Json;
using BattleReady.Api.Models.Requests;
using BattleReady.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BattleReady.Tests.Integration;

public class ParseDamageControllerTests : IClassFixture<ParseDamageTestFactory>
{
    private const string Version = "v1";
    private const string CalculateUrl = $"/api/{Version}/ParseDamage/calculate";
    private const string LoggedEndpoint = $"POST {CalculateUrl}";

    private readonly HttpClient _client;
    private readonly ParseDamageTestFactory _factory;

    public ParseDamageControllerTests(ParseDamageTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Calculate_ValidRequest_Returns200()
    {
        var request = new ParseDamageRequest { Expression = "2d6+3 slashing" };

        var response = await _client.PostAsJsonAsync(CalculateUrl, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Calculate_ValidRequest_LogsRequestToDatabase()
    {
        var request = new ParseDamageRequest { Expression = "2d6+3 slashing" };

        await _client.PostAsJsonAsync(CalculateUrl, request);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var log = db.ApiRequestLogs.FirstOrDefault(x => x.Endpoint == LoggedEndpoint);

        Assert.NotNull(log);
        Assert.Equal(200, log.ResponseStatus);
        Assert.NotEmpty(log.ResponseBody);
    }

    [Fact]
    public async Task Calculate_MissingRequiredFields_Returns400()
    {
        var request = new { };

        var response = await _client.PostAsJsonAsync(CalculateUrl, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}