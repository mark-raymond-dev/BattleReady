using BattleReady.Api.Models.Requests;
using BattleReady.Core.Features.Calculator.Models;

namespace BattleReady.Api.Mapping;

public static class CalculationRequestExtensions
{
    public static CalculationInput ToInput(this CalculationRequest request) => new()
    {
        CharacterName      = request.CharacterName,
        EnemyDefense       = request.EnemyDefense,
        Natural20Upgrades  = request.Natural20Upgrades,
        Natural1Downgrades = request.Natural1Downgrades,
        Notes              = request.Notes,
        Attacks            = request.Attacks.Select(a => a.ToInput()).ToList(),
        DefaultAttack      = request.DefaultAttack?.ToInput(),
    };
}