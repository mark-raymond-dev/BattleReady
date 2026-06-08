namespace BattleReady.Features.Calculator.Models;

public class HitChanceRequest
{
    public int ToHit { get; set; }
    public int Defense { get; set; }
    public bool Natural20Upgrades { get; set; } = true;
    public bool Natural1Downgrades { get; set; } = true;
}