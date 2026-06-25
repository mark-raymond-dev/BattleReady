using BattleReady.Api.Models.Requests;
using BattleReady.Core.Features.Calculator.Models;

namespace BattleReady.Api.Mapping;

public static class CalculationRequestExtensions
{
    public static CalculationInput ToInput(this CalculationRequest request)
    {
        // Map regular attacks (SkillRating = BaseToHit; TargetScore = EnemyDefense from request root).
        var attacks = request.Attacks
            .Select(a => a.ToInput(request.EnemyDefense ?? 0))
            .ToList<AttackInput>();

        // Map spell saves (SkillRating = SaveBonus; TargetScore = SpellDc — both live on SpellSaveRequest).
        // EnemyDefense is intentionally not forwarded here; each spell carries its own SpellDc.
        var spellSaves = request.SpellSaves
            .Select(s => s.ToSpellInput())
            .ToList<AttackInput>();

        return new CalculationInput
        {
            CharacterName      = request.CharacterName,
            Natural20Upgrades  = request.Natural20Upgrades,
            Natural1Downgrades = request.Natural1Downgrades,
            Notes              = request.Notes,
            Attacks            = attacks.Concat(spellSaves).ToList(),
            DefaultAttack      = request.DefaultAttack?.ToInput(request.EnemyDefense ?? 0),
        };
    }
}
