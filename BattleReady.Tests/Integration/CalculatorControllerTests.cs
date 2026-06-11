using System.Net;
using System.Net.Http.Json;
using BattleReady.Api.Models.Requests;
using BattleReady.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BattleReady.Tests.Integration;

public class CalculatorControllerTests : IClassFixture<CalculatorTestFactory>
{
    private readonly HttpClient _client;
    private readonly CalculatorTestFactory _factory;

    public CalculatorControllerTests(CalculatorTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Calculate_ValidRequest_Returns200()
    {
        var request = new CalculationRequest
        {
            EnemyDefense = 19,
            Natural20Upgrades = true,
            Natural1Downgrades = true,
            Attacks = new List<AttackRequest>
            {
                new AttackRequest
                {
                    AttackNumber = 1,
                    BaseToHit = 12,
                    NormalHitDamage = "1d6+6",
                    CritHitDamage = "dbl",
                    NormalMissDamage = "0",
                    CritMissDamage = "0"
                }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/Calculator/calculate", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Calculate_ValidRequest_LogsRequestToDatabase()
    {
        var request = new CalculationRequest
        {
            EnemyDefense = 19,
            Natural20Upgrades = true,
            Natural1Downgrades = true,
            Attacks = new List<AttackRequest>
            {
                new AttackRequest
                {
                    AttackNumber = 1,
                    BaseToHit = 12,
                    NormalHitDamage = "1d6+6",
                    CritHitDamage = "dbl",
                    NormalMissDamage = "0",
                    CritMissDamage = "0"
                }
            }
        };

        await _client.PostAsJsonAsync("/api/Calculator/calculate", request);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var log = db.ApiRequestLogs.FirstOrDefault(x => x.Endpoint == "POST /api/Calculator/calculate");

        Assert.NotNull(log);
        Assert.Equal(200, log.ResponseStatus);
        Assert.NotEmpty(log.ResponseBody);
    }

    [Fact]
    public async Task Calculate_MissingRequiredFields_Returns400()
    {
        var request = new { };

        var response = await _client.PostAsJsonAsync("/api/Calculator/calculate", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}