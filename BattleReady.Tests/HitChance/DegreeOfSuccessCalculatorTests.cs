using BattleReady.Core.Features.Calculator.Models;

namespace BattleReady.Tests.HitChance;

public class DegreeOfSuccessCalculatorTests
{
    // -------------------------------------------------------------------------
    // Baseline degree of success (no Nat 20 / Nat 1 rules)
    // -------------------------------------------------------------------------

    [Fact]
    public void GetDegree_CriticalHit_WhenTotalMeetsDefensePlusTen()
    {
        // toHit 10 + d20 10 = 20, defense 10, tgtCritHit = 20
        var result = DegreeOfSuccessCalculator.GetDegreeOfSuccess(toHit: 10, d20: 10, defense: 10,
                         natural20Upgrades: false, natural1Downgrades: false);
        Assert.Equal(DegreeOfSuccess.CriticalHit, result);
    }

    [Fact]
    public void GetDegree_Hit_WhenTotalMeetsDefenseButNotPlusTen()
    {
        //-----------------------
        // toHit 5 + d20 5 = 10, defense 10, tgtNormHit = 10
        //-----------------------

        // ARRANGE — set up your inputs
        int toHit = 5;
        int d20 = 5;
        int defense = 10;
        bool natural20Upgrades = false;
        bool natural1Downgrades = false;

        // ACT — call the thing you're testing
        var result = DegreeOfSuccessCalculator.GetDegreeOfSuccess(toHit, d20, defense, natural20Upgrades, natural1Downgrades);
        
        // ASSERT — verify the outcome
        Assert.Equal(DegreeOfSuccess.Hit, result);
    }

    [Fact]
    public void GetDegree_Miss_WhenTotalBelowDefenseButNotByTen()
    {
        // toHit 0 + d20 5 = 5, defense 10, tgtNormMiss = 0
        var result = DegreeOfSuccessCalculator.GetDegreeOfSuccess(toHit: 0, d20: 5, defense: 10,
                         natural20Upgrades: false, natural1Downgrades: false);
        Assert.Equal(DegreeOfSuccess.Miss, result);
    }

    [Fact]
    public void GetDegree_CriticalMiss_WhenTotalBelowDefenseByTen()
    {
        // toHit -10 + d20 1 = -9, defense 10, tgtNormMiss = 0
        // total (-9) < tgtNormMiss (0) → CriticalMiss
        var result = DegreeOfSuccessCalculator.GetDegreeOfSuccess(toHit: -10, d20: 1, defense: 10,
                        natural20Upgrades: false, natural1Downgrades: false);
        Assert.Equal(DegreeOfSuccess.CriticalMiss, result);
    }

    // -------------------------------------------------------------------------
    // Natural 20 upgrade rule
    // -------------------------------------------------------------------------

    [Fact]
    public void GetDegree_Nat20UpgradesHitToCriticalHit()
    {
        // toHit 0 + d20 20 = 20, defense 25 → would be a Miss baseline,
        // but Nat 20 rule upgrades Miss → Hit → ... wait, it only upgrades once.
        // Let's set up a clean Hit that upgrades to CritHit.
        // toHit 5 + d20 20 = 25, defense 25 → baseline Hit, upgrades to CritHit
        var result = DegreeOfSuccessCalculator.GetDegreeOfSuccess(toHit: 5, d20: 20, defense: 25,
                         natural20Upgrades: true, natural1Downgrades: false);
        Assert.Equal(DegreeOfSuccess.CriticalHit, result);
    }

    [Fact]
    public void GetDegree_Nat20DoesNotUpgradeAlreadyCriticalHit()
    {
        // toHit 10 + d20 20 = 30, defense 15 → already CritHit, no change
        var result = DegreeOfSuccessCalculator.GetDegreeOfSuccess(toHit: 10, d20: 20, defense: 15,
                         natural20Upgrades: true, natural1Downgrades: false);
        Assert.Equal(DegreeOfSuccess.CriticalHit, result);
    }

    [Fact]
    public void GetDegree_Nat20UpgradeDisabled_NoUpgrade()
    {
        // Same setup as upgrade test but flag off — should stay Hit
        var result = DegreeOfSuccessCalculator.GetDegreeOfSuccess(toHit: 5, d20: 20, defense: 25,
                         natural20Upgrades: false, natural1Downgrades: false);
        Assert.Equal(DegreeOfSuccess.Hit, result);
    }

    // -------------------------------------------------------------------------
    // Natural 1 downgrade rule
    // -------------------------------------------------------------------------

    [Fact]
    public void GetDegree_Nat1DowngradesHitToMiss()
    {
        // toHit 20 + d20 1 = 21, defense 15 → baseline CritHit,
        // downgraded once to Hit. Let's use a baseline Hit → Miss instead.
        // toHit 14 + d20 1 = 15, defense 15 → baseline Hit, downgrades to Miss
        var result = DegreeOfSuccessCalculator.GetDegreeOfSuccess(toHit: 14, d20: 1, defense: 15,
                         natural20Upgrades: false, natural1Downgrades: true);
        Assert.Equal(DegreeOfSuccess.Miss, result);
    }

    [Fact]
    public void GetDegree_Nat1DowngradesCritHitToHit()
    {
        // toHit 24 + d20 1 = 25, defense 15 → baseline CritHit, downgrades to Hit
        var result = DegreeOfSuccessCalculator.GetDegreeOfSuccess(toHit: 24, d20: 1, defense: 15,
                         natural20Upgrades: false, natural1Downgrades: true);
        Assert.Equal(DegreeOfSuccess.Hit, result);
    }

    [Fact]
    public void GetDegree_Nat1DoesNotDowngradeAlreadyCriticalMiss()
    {
        // toHit 0 + d20 1 = 1, defense 20 → baseline CritMiss, can't go lower
        var result = DegreeOfSuccessCalculator.GetDegreeOfSuccess(toHit: 0, d20: 1, defense: 20,
                         natural20Upgrades: false, natural1Downgrades: true);
        Assert.Equal(DegreeOfSuccess.CriticalMiss, result);
    }

    [Fact]
    public void GetDegree_Nat1DowngradeDisabled_NoDowngrade()
    {
        // Same setup as downgrade test but flag off — should stay Hit
        var result = DegreeOfSuccessCalculator.GetDegreeOfSuccess(toHit: 14, d20: 1, defense: 15,
                         natural20Upgrades: false, natural1Downgrades: false);
        Assert.Equal(DegreeOfSuccess.Hit, result);
    }

    // -------------------------------------------------------------------------
    // Boundary conditions
    // -------------------------------------------------------------------------

    [Fact]
    public void GetDegree_ExactlyAtCritHitBoundary()
    {
        // total must be >= defense + 10 for CritHit
        // toHit 9 + d20 11 = 20, defense 10, tgtCritHit = 20 → CritHit
        var result = DegreeOfSuccessCalculator.GetDegreeOfSuccess(toHit: 9, d20: 11, defense: 10,
                         natural20Upgrades: false, natural1Downgrades: false);
        Assert.Equal(DegreeOfSuccess.CriticalHit, result);
    }

    [Fact]
    public void GetDegree_OneBelowCritHitBoundary()
    {
        // toHit 8 + d20 11 = 19, defense 10, tgtCritHit = 20 → Hit
        var result = DegreeOfSuccessCalculator.GetDegreeOfSuccess(toHit: 8, d20: 11, defense: 10,
                         natural20Upgrades: false, natural1Downgrades: false);
        Assert.Equal(DegreeOfSuccess.Hit, result);
    }
}