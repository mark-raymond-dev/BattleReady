namespace BattleReady.Core.Features.Calculator.Models;

// Represents a spell that forces the target to make a saving throw.
// SkillRating = the defender's save bonus (e.g. Reflex, Will, Fortitude).
// TargetScore = the caster's Spell DC.
// IsSpellRequiringSavingThrow is always true for this type — CalculationService
// uses it to branch to SpellSaveService instead of HitChanceService, and to
// skip MAP (saving throw spells are not subject to Multiple Attack Penalty).
public class SpellInput : AttackInput
{
    public SpellInput()
    {
        IsSpellRequiringSavingThrow = true;
        HasMAP = false;
    }
}