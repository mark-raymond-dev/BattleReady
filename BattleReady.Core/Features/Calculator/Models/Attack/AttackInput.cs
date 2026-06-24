namespace BattleReady.Core.Features.Calculator.Models;

public class AttackInput
{

    #region Properties

    public int AttackNumber { get; set; }

    public int SkillRating { get; set; }
    public int TargetScore { get; set; }

    public string NormalHitDamage { get; set; } = string.Empty; 

    public bool IsDefaultAttack { get; set; } = false;
    public bool HasMAP { get; set; } = true;
    public bool IsAgile { get; set; } = false;
    public bool IsSpellRequiringSavingThrow { get; set; } = false;

    public string CritHitDamage { get; set; } = string.Empty; 
    public string NormalMissDamage { get; set; } = string.Empty; 
    public string CritMissDamage { get; set; } = string.Empty;

    #endregion

    #region Methods

    public AttackInput Clone() => new()
    {
        AttackNumber                  = this.AttackNumber,
        IsDefaultAttack               = this.IsDefaultAttack,
        SkillRating                   = this.SkillRating,
        TargetScore                   = this.TargetScore,
        HasMAP                        = this.HasMAP,
        IsAgile                       = this.IsAgile,
        IsSpellRequiringSavingThrow   = this.IsSpellRequiringSavingThrow,
        NormalHitDamage               = this.NormalHitDamage,
        CritHitDamage                 = this.CritHitDamage,
        NormalMissDamage              = this.NormalMissDamage,
        CritMissDamage                = this.CritMissDamage,
    };

    #endregion

}
