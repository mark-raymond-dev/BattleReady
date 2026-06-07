namespace BattleReady.Features.Calculator.Models;

public class HitChance
{

    #region Properties

    public int ToHit { get; set; }
    public int Defense { get; set; }
    public double CritMissChance { get; set; }
    public double NormalMissChance { get; set; }
    public double NormalHitChance { get; set; }
    public double CritHitChance { get; set; }

    #endregion

}