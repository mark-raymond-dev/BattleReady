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
}