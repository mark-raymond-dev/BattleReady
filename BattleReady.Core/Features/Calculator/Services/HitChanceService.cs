namespace BattleReady.Core.Features.Calculator.Services;

using BattleReady.Core.Features.Calculator.Models;

public class HitChanceService : IHitChanceService
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
            var degreeOfSuccess = DegreeOfSuccessCalculator.GetDegreeOfSuccess(toHit, d20, defense, natural20Upgrades, natural1Downgrades);
            switch (degreeOfSuccess)
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

    #endregion

}