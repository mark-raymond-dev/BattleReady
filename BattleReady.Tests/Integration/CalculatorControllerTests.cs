using System.Net;
using System.Net.Http.Json;
using BattleReady.Api.Models.Requests;
using BattleReady.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BattleReady.Tests.Integration;

public class CalculatorControllerTests : IClassFixture<CalculatorTestFactory>
{
    private const string Version = "v1";
    private const string CalculateUrl = $"/api/{Version}/Calculator/calculate";
    private const string LoggedEndpoint = $"POST {CalculateUrl}";

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

        var response = await _client.PostAsJsonAsync(CalculateUrl, request);

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

    [Fact]
    public async Task Calculate_IsAgileWithoutHasMAP_Returns400()
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
                    AttackNumber     = 1,
                    BaseToHit        = 12,
                    NormalHitDamage  = "1d6+6",
                    HasMAP           = false,   // IsAgile=true with HasMAP=false is the invalid combination
                    IsAgile          = true
                }
            }
        };

        var response = await _client.PostAsJsonAsync(CalculateUrl, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Calculate_IsDefaultAttackSetWithNoDefaultAttack_Returns400()
    {
        var request = new CalculationRequest
        {
            EnemyDefense = 19,
            Natural20Upgrades = true,
            Natural1Downgrades = true,
            DefaultAttack = null,   // no default provided
            Attacks = new List<AttackRequest>
            {
                new AttackRequest
                {
                    AttackNumber     = 1,
                    BaseToHit        = 12,
                    NormalHitDamage  = "1d6+6",
                    IsDefaultAttack  = true     // but this attack wants to use one
                }
            }
        };

        var response = await _client.PostAsJsonAsync(CalculateUrl, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Calculate_ValidRequestUsingDefaultAttack_Returns200()
    {
        // This was the bug: a request using DefaultAttack + IsDefaultAttack=true
        // was incorrectly rejected because [Range] fired on DefaultAttack.AttackNumber
        // and [Required] fired on the concrete attacks' missing NormalHitDamage.
        // Both rules are now context-aware and only apply where semantically correct.
        var request = new CalculationRequest
        {
            EnemyDefense = 19,
            Natural20Upgrades = true,
            Natural1Downgrades = true,
            DefaultAttack = new DefaultAttackRequest
            {
                BaseToHit       = 12,
                NormalHitDamage = "1d6+6 slashing",
                CritHitDamage   = "dbl",
                NormalMissDamage = "0",
                CritMissDamage  = "0"
            },
            Attacks = new List<AttackRequest>
            {
                new AttackRequest { AttackNumber = 1, IsDefaultAttack = true },
                new AttackRequest { AttackNumber = 2, IsDefaultAttack = true },
                new AttackRequest { AttackNumber = 3, IsDefaultAttack = true }
            }
        };

        var response = await _client.PostAsJsonAsync(CalculateUrl, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Calculate_DefaultAttackMissingNormalHitDamage_Returns400()
    {
        // The DefaultAttack template must supply NormalHitDamage — concrete attacks
        // that use IsDefaultAttack=true inherit it from here.
        var request = new CalculationRequest
        {
            EnemyDefense = 19,
            Natural20Upgrades = true,
            Natural1Downgrades = true,
            DefaultAttack = new DefaultAttackRequest
            {
                BaseToHit        = 12,
                NormalHitDamage  = ""   // missing — should fail
            },
            Attacks = new List<AttackRequest>
            {
                new AttackRequest { AttackNumber = 1, IsDefaultAttack = true }
            }
        };

        var response = await _client.PostAsJsonAsync(CalculateUrl, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Calculate_ConcreteAttackMissingNormalHitDamage_Returns400()
    {
        // A concrete attack (IsDefaultAttack=false) must supply its own NormalHitDamage.
        var request = new CalculationRequest
        {
            EnemyDefense = 19,
            Natural20Upgrades = true,
            Natural1Downgrades = true,
            Attacks = new List<AttackRequest>
            {
                new AttackRequest
                {
                    AttackNumber    = 1,
                    BaseToHit       = 12,
                    NormalHitDamage = ""    // missing — should fail
                }
            }
        };

        var response = await _client.PostAsJsonAsync(CalculateUrl, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Calculate_ConcreteAttackOutOfRangeAttackNumber_Returns400()
    {
        // AttackNumber range (1-20) is still enforced for concrete attacks.
        var request = new CalculationRequest
        {
            EnemyDefense = 19,
            Natural20Upgrades = true,
            Natural1Downgrades = true,
            Attacks = new List<AttackRequest>
            {
                new AttackRequest
                {
                    AttackNumber    = 99,   // out of range — should fail
                    BaseToHit       = 12,
                    NormalHitDamage = "1d6+6"
                }
            }
        };

        var response = await _client.PostAsJsonAsync(CalculateUrl, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}