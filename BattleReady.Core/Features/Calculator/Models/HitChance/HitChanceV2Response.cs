namespace BattleReady.Core.Features.Calculator.Models;

// v2 response — drops the ToHit/Defense echo-back fields that v1 included.
// The caller already has those values; returning them wastes bytes and creates
// a misleading suggestion that the response is "opinionated" about what was sent.
public class HitChanceV2Response
{
    public double CritMissChance   { get; set; }
    public double NormalMissChance { get; set; }
    public double NormalHitChance  { get; set; }
    public double CritHitChance    { get; set; }
}
