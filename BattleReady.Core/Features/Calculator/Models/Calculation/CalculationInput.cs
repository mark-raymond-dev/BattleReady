namespace BattleReady.Core.Features.Calculator.Models;

public class CalculationInput
{

    #region Properties

    public List<AttackInput> Attacks { get; set; } = [];

    public string? CharacterName { get; set; } 
    public AttackInput? DefaultAttack { get; set; } = null;
    public string? Notes { get; set; }
    public bool Natural20Upgrades { get; set; } = true;
    public bool Natural1Downgrades { get; set; } = true;

    #endregion

}
