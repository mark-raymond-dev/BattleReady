using BattleReady.Core.Features.Calculator.Services;

namespace BattleReady.Tests.HitChance;

public class HitChanceServiceTests
{
    private readonly HitChanceService _service = new();

    [Fact]
    public void Calculate_ChancesSumToOneHundredPercent()
    {
        var result = _service.Calculate(toHit: 5, defense: 15);

        var total = result.CritHitChance + result.NormalHitChance
                  + result.NormalMissChance + result.CritMissChance;

        Assert.Equal(1.0, total, precision: 10);
    }

    [Fact]
    public void Calculate_AllCritHits_WhenToHitFarExceedsDefense()
    {
        // toHit 100 + any d20 roll (1-20) always meets defense + 10
        // Nat 1 downgrade is the only thing that could interfere,
        // dropping d20=1 from CritHit to Hit — so disable it.
        var result = _service.Calculate(toHit: 100, defense: 15,
                         natural20Upgrades: false, natural1Downgrades: false);

        Assert.Equal(1.0, result.CritHitChance, precision: 10);
        Assert.Equal(0.0, result.NormalHitChance, precision: 10);
        Assert.Equal(0.0, result.NormalMissChance, precision: 10);
        Assert.Equal(0.0, result.CritMissChance, precision: 10);
    }
}