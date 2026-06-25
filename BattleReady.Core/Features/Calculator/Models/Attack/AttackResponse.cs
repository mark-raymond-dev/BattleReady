namespace BattleReady.Core.Features.Calculator.Models;

public class AttackResponse
{

    #region Properties

    public int AttackNumber { get; set; } 
    public int EffectiveSkillRating { get; set; } 
    public int EffectiveTargetScore { get; set; }
    
    public double CritMissChance { get; set; }
    public double NormalMissChance { get; set; }
    public double NormalHitChance { get; set; }
    public double CritHitChance { get; set; }
    
    public double AvgDmgCritMiss { get; set; } 
    public double AvgDmgNormalMiss { get; set; } 
    public double AvgDmgNormalHit { get; set; } 
    public double AvgDmgCritHit { get; set; } 
    
    public double TotalExpectedDamage { get; set; }

    #endregion

}
