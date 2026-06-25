namespace BattleReady.Core.Features.Calculator.Models;

// Spell save response from the CASTER's perspective.
// The defender rolls against the caster's Spell DC — a high defender roll
// is bad for the caster, so the buckets are inverted relative to DegreeOfSuccessResponse.
// SkillRating (SaveBonus) and TargetScore (SpellDc) are omitted — they are inputs
// the caller already has, not results of the calculation.
public class SpellSaveResponse
{
    public double CritMissChance   { get; set; }    // defender critically succeeded
    public double NormalMissChance { get; set; }    // defender succeeded (typically half damage)
    public double NormalHitChance  { get; set; }    // defender failed (typically full damage)
    public double CritHitChance    { get; set; }    // defender critically failed (typically double damage)
}