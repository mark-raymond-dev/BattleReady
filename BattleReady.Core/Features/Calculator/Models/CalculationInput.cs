namespace BattleReady.Features.Calculator.Models;

public class CalculationInput
{

    #region Properties

    public int EnemyDefense { get; set; }

    public List<AttackRequest> Attacks { get; set; } = [];

    public string? CharacterName { get; set; } 
    public AttackRequest? DefaultAttack { get; set; } = null;
    public string? Notes { get; set; }
    public bool Natural20Upgrades { get; set; } = true;
    public bool Natural1Downgrades { get; set; } = true;

    #endregion

}
