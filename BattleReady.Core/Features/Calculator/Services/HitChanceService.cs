namespace BattleReady.Core.Features.Calculator.Services;

using BattleReady.Core.Features.Calculator.Models;

public class HitChanceService
{

    #region Methods

    public HitChanceResponse Calculate(int toHit, int defense, bool natural20Upgrades = true, bool natural1Downgrades = true)
    {
        // Loop through every possible d20 roll (1 to 20) and
        // count how many times each degree of success occurs.
        int critMissCount = 0;
        int missCount = 0;
        int hitCount = 0;
        int critHitCount = 0;
        for (int d20 = 1; d20 <= 20; d20++)
        {
            var degree = GetDegree(toHit, d20, defense, natural20Upgrades, natural1Downgrades);
            switch (degree)
            {
                case DegreeOfSuccess.CriticalHit: critHitCount++; break;
                case DegreeOfSuccess.Hit: hitCount++; break;
                case DegreeOfSuccess.Miss: missCount++; break;
                case DegreeOfSuccess.CriticalMiss: critMissCount++; break;
            }
        }

        var hitChanceResponse = new HitChanceResponse
        {
            ToHit = toHit,
            Defense = defense,
            // Convert the counts into percentages (Each roll = 5%)            
            CritMissChance = critMissCount * 0.05,
            NormalMissChance = missCount * 0.05,
            NormalHitChance = hitCount * 0.05,
            CritHitChance = critHitCount * 0.05
        };
        return hitChanceResponse;
    }

    public static DegreeOfSuccess GetDegree(int toHit, int d20, int defense, bool natural20Upgrades = true, bool natural1Downgrades = true)
    {
        // Determine total attack roll and target numbers for each degree of success.
        int total = d20 + toHit;
        int tgtCritHit = defense + 10;
        int tgtNormHit = defense;
        int tgtNormMiss = defense - 10;

        // Determine the base degree of success.
        var degree = DegreeOfSuccess.CriticalMiss; // default
        if (total >= tgtCritHit)
        {
            degree = DegreeOfSuccess.CriticalHit;
        }
        else if (total >= tgtNormHit)
        {
            degree = DegreeOfSuccess.Hit;
        }
        else if (total >= tgtNormMiss)
        {
            degree = DegreeOfSuccess.Miss;
        }

        // Apply Nat 20 / 1 rule adjustments (if applicable).
        if (natural20Upgrades && d20 == 20 && degree < DegreeOfSuccess.CriticalHit) degree++; // Upgrade on Natural 20
        if (natural1Downgrades && d20 == 1 && degree > DegreeOfSuccess.CriticalMiss)  degree--; // Downgrade on Natural 1

        return degree;
    }

    #endregion

}