using System.ComponentModel.DataAnnotations;

namespace BattleReady.Api.Models.Requests;

// A template attack that concrete attacks can inherit from via IsDefaultAttack = true.
// Intentionally excludes AttackNumber (irrelevant for a template) and IsDefaultAttack
// (only meaningful on concrete attacks that reference this template).
// Validation rules are simpler here — NormalHitDamage is unconditionally required
// because a template that omits it would be useless.
public class DefaultAttackRequest : IValidatableObject
{

    #region Properties with DataAnnotations

    [Required]
    [Range(-20, 50, ErrorMessage = "BaseToHit must be between -20 and 50.")]
    public int? BaseToHit { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "NormalHitDamage is required.")]
    public string NormalHitDamage { get; set; } = string.Empty;

    #endregion

    #region Properties without DataAnnotations

    public bool HasMAP { get; set; } = true;
    public bool IsAgile { get; set; } = false;
    public bool IsSpellRequiringSavingThrow { get; set; } = false;

    public string CritHitDamage { get; set; } = string.Empty;
    public string NormalMissDamage { get; set; } = string.Empty;
    public string CritMissDamage { get; set; } = string.Empty;

    #endregion

    #region Validation

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // IsAgile has no meaning unless MAP is involved.
        if (IsAgile && !HasMAP)
            yield return new ValidationResult(
                "IsAgile has no effect when HasMAP is false. Either set HasMAP to true, or set IsAgile to false.",
                new[] { nameof(IsAgile), nameof(HasMAP) });
    }

    #endregion

}