using System.ComponentModel.DataAnnotations;

namespace BattleReady.Api.Models.Requests;

// Unlike DefaultAttackRequest, this has a property for AttackNumber which is
// required and must be between 1 and 20. 
public class AttackRequest : IValidatableObject
{

    #region Properties with DataAnnotations

    [Range(1, 20, ErrorMessage = "AttackNumber must be between 1 and 20.")]
    public int AttackNumber { get; set; }

    #endregion

    #region Properties without DataAnnotations

    // We are purposefully opting to NOT have DataAnnotations for these two properties,
    // because they are actually not required if IsDefaultAttack is set to true.
    public int? BaseToHit { get; set; }
    public string NormalHitDamage { get; set; } = string.Empty;

    public bool IsDefaultAttack { get; set; } = false;
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

        // If this isn't using the default attack template,
        // BaseToHit and NormalHitDamage are required.
        if (!IsDefaultAttack)
        {
            if (BaseToHit == null)
            {
                yield return new ValidationResult(
                    "BaseToHit is required.",
                    new[] { nameof(BaseToHit) });
            }
            else if (BaseToHit < -20 || BaseToHit > 50)
            {
                yield return new ValidationResult(
                    "BaseToHit must be between -20 and 50.",
                    new[] { nameof(BaseToHit) });
            }

            if (string.IsNullOrWhiteSpace(NormalHitDamage))
            {
                yield return new ValidationResult(
                    "NormalHitDamage is required.",
                    new[] { nameof(NormalHitDamage) });
            }
        }
    }

    #endregion

}