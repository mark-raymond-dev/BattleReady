namespace BattleReady.Features.Calculator.Models;

public class CalculationRequest
{

    #region Properties

    public string? CharacterName { get; set; } 
    public int EnemyDefense { get; set; }
    public List<AttackInput> Attacks { get; set; } = [];
    public AttackInput? DefaultAttack { get; set; } = null;
    public string? Notes { get; set; }
    public bool Natural20Upgrades { get; set; } = true;
    public bool Natural1Downgrades { get; set; } = true;

    #endregion

}
