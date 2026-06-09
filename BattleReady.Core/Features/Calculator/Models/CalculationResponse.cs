namespace BattleReady.Core.Features.Calculator.Models;

using System.Text;

public class CalculationResponse
{

    #region Properties

    public List<AttackResponse> AttackResponses { get; set; } = [];
    public double TotalExpectedDamageAllAttacks { get; set; }
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;

    #endregion

    #region ToString

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"\n\nCalculated at {CalculatedAt:u}\n\n");
        foreach (var ar in AttackResponses.OrderBy(ar => ar.AttackNumber))
        {
            sb.AppendLine($"Attack {ar.AttackNumber}  .....  Eff To-Hit: {ar.EffectiveToHit}, Defense: {ar.EffectiveDefense}");
            sb.AppendLine($"% Chc (Dmg) [ CritHit: {ar.CritHitChance:P2} ({ar.AvgDmgCritHit:F2}), Hit: {ar.NormalHitChance:P2} ({ar.AvgDmgNormalHit:F2}), Miss: {ar.NormalMissChance:P2} ({ar.AvgDmgNormalMiss:F2}), CritMiss: {ar.CritMissChance:P2} ({ar.AvgDmgCritMiss:F2}) ]");
            sb.AppendLine($"Total Expected Dmg for this Attack: {ar.TotalExpectedDamage:F2}\n");
        }
        sb.AppendLine($"Total Expected Damage for All Attacks: {TotalExpectedDamageAllAttacks:F2}\n");
        return sb.ToString();
    }

    #endregion

}
