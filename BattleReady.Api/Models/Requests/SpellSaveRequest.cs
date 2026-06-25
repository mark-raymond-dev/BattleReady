using System.ComponentModel.DataAnnotations;

namespace BattleReady.Api.Models.Requests;

// Represents a spell that forces a saving throw.
// SaveBonus = the defender's save bonus (Reflex, Will, or Fortitude).
// SpellDc   = the caster's Spell DC.
// AttackNumber is included so the caller can control ordering in a mixed turn
// alongside regular AttackRequests. Defaults to 0; CalculationService will
// renumber if needed.
public class SpellSaveRequest
{

    #region Properties with DataAnnotations

    [Required]
    [Range(-20, 50, ErrorMessage = "SaveBonus must be between -20 and 50.")]
    public int? SaveBonus { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "SpellDc must be between 1 and 100.")]
    public int? SpellDc { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "NormalHitDamage is required.")]
    public string NormalHitDamage { get; set; } = string.Empty;

    #endregion

    #region Properties without DataAnnotations

    public int AttackNumber { get; set; } = 0;

    public string CritHitDamage { get; set; } = string.Empty;
    public string NormalMissDamage { get; set; } = string.Empty;
    public string CritMissDamage { get; set; } = string.Empty;

    #endregion

}
