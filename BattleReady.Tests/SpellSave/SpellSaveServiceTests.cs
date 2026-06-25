using BattleReady.Core.Features.Calculator.Services;

namespace BattleReady.Tests.SpellSaveTests;

public class SpellSaveServiceTests
{
    private readonly SpellSaveService _service = new(new DegreeOfSuccessService());

    [Fact]
    public void Calculate_ChancesSumToOneHundredPercent()
    {
        var result = _service.Calculate(saveBonus: 5, spellDc: 15);

        var total = result.CritHitChance   + result.NormalHitChance
                  + result.NormalMissChance + result.CritMissChance;

        Assert.Equal(1.0, total, precision: 10);
    }

    [Fact]
    public void Calculate_BucketsAreInvertedRelativeToDegreeOfSuccess()
    {
        // When the defender has no chance of succeeding, the caster should
        // have 100% crit hit chance (defender always critically fails).
        // saveBonus -100 + any d20 (1-20) can never reach spellDc 15 - 10 = 5.
        // Disable nat 20 upgrade so no roll can accidentally succeed.
        var result = _service.Calculate(
            saveBonus:          -100,
            spellDc:            15,
            natural20Upgrades:  false,
            natural1Downgrades: false);

        Assert.Equal(1.0, result.CritHitChance,    precision: 10);
        Assert.Equal(0.0, result.NormalHitChance,  precision: 10);
        Assert.Equal(0.0, result.NormalMissChance, precision: 10);
        Assert.Equal(0.0, result.CritMissChance,   precision: 10);
    }

    [Fact]
    public void Calculate_AllCritMiss_WhenDefenderAlwaysCriticallySucceeds()
    {
        // saveBonus 100 + any d20 (1-20) always meets spellDc + 10.
        // Disable nat 1 downgrade so no roll can accidentally fail.
        var result = _service.Calculate(
            saveBonus:          100,
            spellDc:            15,
            natural20Upgrades:  false,
            natural1Downgrades: false);

        Assert.Equal(0.0, result.CritHitChance,    precision: 10);
        Assert.Equal(0.0, result.NormalHitChance,  precision: 10);
        Assert.Equal(0.0, result.NormalMissChance, precision: 10);
        Assert.Equal(1.0, result.CritMissChance,   precision: 10);
    }
}