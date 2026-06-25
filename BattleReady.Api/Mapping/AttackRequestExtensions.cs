using BattleReady.Api.Models.Requests;
using BattleReady.Core.Features.Calculator.Models;

namespace BattleReady.Api.Mapping;

public static class AttackRequestExtensions
{
    public static AttackInput ToInput(this AttackRequest request, int targetScore = 0) => new()
    {
        AttackNumber                = request.AttackNumber,
        IsDefaultAttack             = request.IsDefaultAttack,
        SkillRating                 = request.BaseToHit ?? 0,
        TargetScore                 = targetScore,
        HasMAP                      = request.HasMAP,
        IsAgile                     = request.IsAgile,
        IsSpellRequiringSavingThrow = request.IsSpellRequiringSavingThrow,
        NormalHitDamage             = request.NormalHitDamage,
        CritHitDamage               = request.CritHitDamage,
        NormalMissDamage            = request.NormalMissDamage,
        CritMissDamage              = request.CritMissDamage,
    };

    public static AttackInput ToInput(this DefaultAttackRequest request, int targetScore = 0) => new()
    {
        AttackNumber                = 0,    // templates have no meaningful AttackNumber
        IsDefaultAttack             = false,
        SkillRating                 = request.BaseToHit ?? 0,
        TargetScore                 = targetScore,
        HasMAP                      = request.HasMAP,
        IsAgile                     = request.IsAgile,
        IsSpellRequiringSavingThrow = request.IsSpellRequiringSavingThrow,
        NormalHitDamage             = request.NormalHitDamage,
        CritHitDamage               = request.CritHitDamage,
        NormalMissDamage            = request.NormalMissDamage,
        CritMissDamage              = request.CritMissDamage,
    };

    // SpellSaveRequest maps to SpellInput (a marker subclass of AttackInput).
    // SpellInput's constructor pre-sets IsSpellRequiringSavingThrow = true and HasMAP = false.
    // SkillRating = SaveBonus (the defender rolls this).
    // TargetScore = SpellDc   (the caster's DC the defender must beat).
    public static SpellInput ToSpellInput(this SpellSaveRequest request) => new()
    {
        AttackNumber    = request.AttackNumber,
        SkillRating     = request.SaveBonus ?? 0,
        TargetScore     = request.SpellDc   ?? 0,
        NormalHitDamage = request.NormalHitDamage,
        CritHitDamage   = request.CritHitDamage,
        NormalMissDamage = request.NormalMissDamage,
        CritMissDamage  = request.CritMissDamage,
    };
}