using System.ComponentModel.DataAnnotations;

namespace BattleReady.Api.Models.Requests;

public class CalculationRequest : IValidatableObject
{

    #region Properties with DataAnnotations (e.g. Required or Validation)

    [Required]
    [Range(1, 100, ErrorMessage = "EnemyDefense must be between 1 and 100.")]
    public int? EnemyDefense { get; set; }

    // Attacks and SpellSaves are both optional independently, but at least one
    // entry across both lists is required — enforced in Validate() below.
    public List<AttackRequest> Attacks { get; set; } = [];
    public List<SpellSaveRequest> SpellSaves { get; set; } = [];

    #endregion

    #region Properties without DataAnnotations

    public string? CharacterName { get; set; } 
    public DefaultAttackRequest? DefaultAttack { get; set; } = null;
    public string? Notes { get; set; }
    public bool Natural20Upgrades { get; set; } = true;
    public bool Natural1Downgrades { get; set; } = true;

    #endregion

    #region Validation

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // At least one attack or spell save is required.
        if (Attacks.Count == 0 && SpellSaves.Count == 0)
            yield return new ValidationResult(
                "At least one entry in Attacks or SpellSaves is required.",
                new[] { nameof(Attacks), nameof(SpellSaves) });

        // Cannot have Attacks marked as using "default attack" if the DefaultAttack wasn't defined.
        if (Attacks.Any(a => a.IsDefaultAttack) && DefaultAttack is null)
            yield return new ValidationResult(
                "DefaultAttack is required when any attack has IsDefaultAttack set to true.",
                new[] { nameof(DefaultAttack) });
    }  

    #endregion

}