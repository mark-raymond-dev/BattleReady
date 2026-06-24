using BattleReady.Api.Models.Requests;
using BattleReady.Core.Features.Calculator.Models;

namespace BattleReady.Api.Mapping;

public static class CalculationRequestExtensions
{
    public static CalculationInput ToInput(this CalculationRequest request) => new()
    {
        CharacterName      = request.CharacterName,
        Natural20Upgrades  = request.Natural20Upgrades,
        Natural1Downgrades = request.Natural1Downgrades,
        Notes              = request.Notes,
        Attacks            = request.Attacks.Select(a => a.ToInput(request.EnemyDefense ?? 0)).ToList(),
        DefaultAttack      = request.DefaultAttack?.ToInput(request.EnemyDefense ?? 0),
    };
}