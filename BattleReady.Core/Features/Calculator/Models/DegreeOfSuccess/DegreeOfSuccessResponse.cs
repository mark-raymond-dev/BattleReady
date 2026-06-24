namespace BattleReady.Core.Features.Calculator.Models;

// Generic degree-of-success response using domain-neutral terminology.
// SkillRating and TargetScore are omitted intentionally — they are inputs
// the caller already has, not results of the calculation.
public class DegreeOfSuccessResponse
{
    public double CritFailChance    { get; set; }
    public double NormalFailChance  { get; set; }
    public double NormalSuccessChance { get; set; }
    public double CritSuccessChance { get; set; }
}