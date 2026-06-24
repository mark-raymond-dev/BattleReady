namespace BattleReady.Core.Features.Calculator.Services;

using BattleReady.Core.Features.Calculator.Models;

public class DegreeOfSuccessService : IDegreeOfSuccessService
{

    #region Methods

    public DegreeOfSuccessResponse Calculate(
        int skillRating,
        int targetScore,
        bool natural20Upgrades  = true,
        bool natural1Downgrades = true)
    {
        // Loop through every possible d20 roll (1 to 20) and
        // count how many times each degree of success occurs.
        int critFailCount    = 0;
        int normalFailCount  = 0;
        int normalSuccessCount = 0;
        int critSuccessCount = 0;

        for (int d20 = 1; d20 <= 20; d20++)
        {
            var degree = DegreeOfSuccessCalculator.GetDegreeOfSuccess(
                skillRating, d20, targetScore, natural20Upgrades, natural1Downgrades);

            switch (degree)
            {
                case DegreeOfSuccess.CriticalHit:  critSuccessCount++;  break;
                case DegreeOfSuccess.Hit:          normalSuccessCount++; break;
                case DegreeOfSuccess.Miss:         normalFailCount++;   break;
                case DegreeOfSuccess.CriticalMiss: critFailCount++;     break;
            }
        }

        return new DegreeOfSuccessResponse
        {
            // Convert counts to percentages (each roll = 5%)
            CritFailChance     = critFailCount    * 0.05,
            NormalFailChance   = normalFailCount  * 0.05,
            NormalSuccessChance = normalSuccessCount * 0.05,
            CritSuccessChance  = critSuccessCount * 0.05,
        };
    }

    #endregion

}