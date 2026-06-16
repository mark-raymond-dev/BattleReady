using BattleReady.Core.Features.Calculator.Models;

namespace BattleReady.Tests.HitChance;

public class DegreeOfSuccessCalculatorTests
{
    [Theory]
    // Baseline degree of success (no Nat 20 / Nat 1 rules)
    [InlineData(10, 10, 10, false, false, DegreeOfSuccess.CriticalHit)]     // total 20, defense 10, tgtCritHit = 20
    [InlineData(5, 5, 10, false, false, DegreeOfSuccess.Hit)]               // total 10, defense 10, tgtNormHit = 10
    [InlineData(0, 5, 10, false, false, DegreeOfSuccess.Miss)]              // total  5, defense 10, tgtNormMiss = 0
    [InlineData(-2, 1, 10, false, false, DegreeOfSuccess.CriticalMiss)]     // total -1, defense 10, below tgtNormMiss (0)
    // Natural 20 upgrade rule
    [InlineData(5, 20, 25, true, false, DegreeOfSuccess.CriticalHit)]       // total 25, defense 25, baseline = Hit, but Nat20 upgrades to CritHit
    [InlineData(10, 20, 15, true, false, DegreeOfSuccess.CriticalHit)]      // total 30, defense 15, baseline = CritHit, Nat20 has no further effect
    [InlineData(5, 20, 25, false, false, DegreeOfSuccess.Hit)]              // total 25, defense 25, baseline = Hit, flag off → stays Hit
    // Natural 1 downgrade rule
    [InlineData(14, 1, 15, false, true, DegreeOfSuccess.Miss)]              // total 15, defense 15, baseline = Hit, but Nat1 downgrades to Miss
    [InlineData(24, 1, 15, false, true, DegreeOfSuccess.Hit)]               // total 25, defense 15, baseline = CritHit, Nat1 downgrades to Hit
    [InlineData(0, 1, 20, false, true, DegreeOfSuccess.CriticalMiss)]       // total  1, defense 20, baseline = CritMiss, Nat1 has no further effect
    [InlineData(14, 1, 15, false, false, DegreeOfSuccess.Hit)]              // total 15, defense 15, baseline = Hit, flag off → stays Hit
    // Boundary conditions
    [InlineData(9, 11, 10, false, false, DegreeOfSuccess.CriticalHit)]      // total 20, defense 10, tgtCritHit = 20 (right at boundary, it is CritHit)
    [InlineData(8, 11, 10, false, false, DegreeOfSuccess.Hit)]              // total 19, defense 10, tgtCritHit = 20 (one below boundary, so just a Hit)
    public void GetDegreeOfSuccess_ReturnsExpectedDegree(
        int toHit, int d20, int defense,
        bool natural20Upgrades, bool natural1Downgrades,
        DegreeOfSuccess expected)
    {
        var result = DegreeOfSuccessCalculator.GetDegreeOfSuccess(
            toHit, d20, defense, natural20Upgrades, natural1Downgrades);

        Assert.Equal(expected, result);
    }
}