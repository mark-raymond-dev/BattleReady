using BattleReady.Api.Models.Requests;
using BattleReady.Core.Features.Calculator.Models;

namespace BattleReady.Api.Mapping;

public static class AttackRequestExtensions
{
    public static AttackInput ToInput(this AttackRequest request) => new()
    {
        AttackNumber                = request.AttackNumber,
        IsDefaultAttack             = request.IsDefaultAttack,
        BaseToHit                   = request.BaseToHit,
        HasMAP                      = request.HasMAP,
        IsAgile                     = request.IsAgile,
        IsSpellRequiringSavingThrow = request.IsSpellRequiringSavingThrow,
        NormalHitDamage             = request.NormalHitDamage,
        CritHitDamage               = request.CritHitDamage,
        NormalMissDamage            = request.NormalMissDamage,
        CritMissDamage              = request.CritMissDamage,
    };
}