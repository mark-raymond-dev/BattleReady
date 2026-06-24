using BattleReady.Core.Features.Calculator.Services;

namespace BattleReady.Tests.DegreeOfSuccessTests;

public class DegreeOfSuccessServiceTests
{
    private readonly DegreeOfSuccessService _service = new();

    [Fact]
    public void Calculate_ChancesSumToOneHundredPercent()
    {
        var result = _service.Calculate(skillRating: 5, targetScore: 15);

        var total = result.CritSuccessChance + result.NormalSuccessChance
                  + result.NormalFailChance  + result.CritFailChance;

        Assert.Equal(1.0, total, precision: 10);
    }

    [Fact]
    public void Calculate_AllCritSuccess_WhenSkillRatingFarExceedsTargetScore()
    {
        // skillRating 100 + any d20 roll (1-20) always meets targetScore + 10.
        // Nat 1 downgrade is the only thing that could interfere — disable it.
        var result = _service.Calculate(
            skillRating:        100,
            targetScore:        15,
            natural20Upgrades:  false,
            natural1Downgrades: false);

        Assert.Equal(1.0, result.CritSuccessChance,   precision: 10);
        Assert.Equal(0.0, result.NormalSuccessChance, precision: 10);
        Assert.Equal(0.0, result.NormalFailChance,    precision: 10);
        Assert.Equal(0.0, result.CritFailChance,      precision: 10);
    }

    [Fact]
    public void Calculate_AllCritFail_WhenSkillRatingFarBelowTargetScore()
    {
        // skillRating -100 + any d20 roll (1-20) never reaches targetScore - 10.
        // Nat 20 upgrade is the only thing that could interfere — disable it.
        var result = _service.Calculate(
            skillRating:        -100,
            targetScore:        15,
            natural20Upgrades:  false,
            natural1Downgrades: false);

        Assert.Equal(0.0, result.CritSuccessChance,   precision: 10);
        Assert.Equal(0.0, result.NormalSuccessChance, precision: 10);
        Assert.Equal(0.0, result.NormalFailChance,    precision: 10);
        Assert.Equal(1.0, result.CritFailChance,      precision: 10);
    }
}