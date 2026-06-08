using System.ComponentModel.DataAnnotations;

namespace BattleReady.Features.Calculator.Models;

public class AttackRequest
{

    #region Properties with DataAnnotations (e.g. Required or Validation)

    [Range(1, 20, ErrorMessage = "AttackNumber must be between 1 and 20.")]
    public int AttackNumber { get; set; }

    [Range(-20, 50, ErrorMessage = "BaseToHit must be between -20 and 50.")]
    public int BaseToHit { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "NormalHitDamage is required.")]
    public string NormalHitDamage { get; set; } = string.Empty; 

    #endregion

    #region Properties without DataAnnotations

    public bool IsDefaultAttack { get; set; } = false;
    public bool HasMAP { get; set; } = true;
    public bool IsAgile { get; set; } = false;
    public bool IsSpellRequiringSavingThrow { get; set; } = false;

    public string CritHitDamage { get; set; } = string.Empty; 
    public string NormalMissDamage { get; set; } = string.Empty; 
    public string CritMissDamage { get; set; } = string.Empty;

    #endregion

    #region Methods

    public AttackRequest Clone() => new()
    {
        AttackNumber                  = this.AttackNumber,
        IsDefaultAttack               = this.IsDefaultAttack,
        BaseToHit                     = this.BaseToHit,
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
