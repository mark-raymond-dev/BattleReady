using System.ComponentModel.DataAnnotations;

namespace BattleReady.Features.Calculator.Models;

public class CalculationRequest
{

    #region Properties with DataAnnotations (e.g. Required or Validation)

    [Required]
    [Range(1, 100, ErrorMessage = "EnemyDefense must be between 1 and 100.")]
    public int EnemyDefense { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Attacks must include at least one attack.")]
    public List<AttackInput> Attacks { get; set; } = [];

    #endregion

    #region Properties without DataAnnotations

    public string? CharacterName { get; set; } 
    public AttackInput? DefaultAttack { get; set; } = null;
    public string? Notes { get; set; }
    public bool Natural20Upgrades { get; set; } = true;
    public bool Natural1Downgrades { get; set; } = true;

    #endregion

}
