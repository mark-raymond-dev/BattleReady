namespace BattleReady.Core.Features.Calculator.Services;

using BattleReady.Core.Features.Calculator.Models;

public interface IDegreeOfSuccessService
{
    DegreeOfSuccessResponse Calculate(
        int skillRating,
        int targetScore,
        bool natural20Upgrades  = true,
        bool natural1Downgrades = true);
}