namespace BattleReady.Core.Features.Calculator.Models;

public class ParseDamageResponse
{

    #region Properties

    public string OriginalExpression { get; set; } = string.Empty;  // e.g. "2d6+3 slashing"
    public int DamageDieCount { get; set; }                         // e.g. the "2" in "2d6+3 slashing"
    public int DamageDieBase { get; set; }                          // e.g. the "6" in "2d6+3 slashing"
    public int DamageModifier { get; set; }                         // e.g. the "+3" in "2d6+3 slashing"
    public string DamageType { get; set; } = string.Empty;          // e.g. the "slashing" in "2d6+3 slashing" (optional)
    public string ParseStatus { get; set; } = "Unparsed";           // e.g. "Parsed", "Error: Invalid Format", etc.
    public double AverageDamage { get; set; }                       // e.g. Avg dmg calc from expression (assuming normal hit)

    #endregion

}