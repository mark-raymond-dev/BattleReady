namespace BattleReady.Features.Calculator.Services;

using System.Text.RegularExpressions;
using BattleReady.Features.Calculator.Models;

public class ParseDamageService
{

    #region Methods

    public ParseDamage Calculate(string damageExpression)
    {
        var parseDamage = new ParseDamage
        {
            OriginalExpression = damageExpression,
            AverageDamage = -1, // Default to -1 to indicate an error if parsing fails
            ParseStatus = "Unparsed"
        };

        if (string.IsNullOrWhiteSpace(damageExpression))
        {
            parseDamage.ParseStatus = "Error: Empty damage expression";
            return parseDamage;
        }

        // Clean up casing and extra spaces, but KEEP single spaces between words
        var normalizedDamageExpression = Regex.Replace(damageExpression.Trim().ToLower(), @"\s+", " ");

        // 1. Handle flat damage with optional damage type (e.g., "5" or "5 slashing")
        var flatMatch = ParseDamage.FlatPattern.Match(normalizedDamageExpression);
        
        if (flatMatch.Success)
        {
            int flatDamage = int.Parse(flatMatch.Groups[1].Value);
            string damageType = flatMatch.Groups[2].Success ? flatMatch.Groups[2].Value : "untyped";

            parseDamage.DamageDieCount = 0;
            parseDamage.DamageDieBase = 0;
            parseDamage.DamageModifier = flatDamage;
            parseDamage.DamageType = damageType;
            parseDamage.AverageDamage = flatDamage;
            parseDamage.ParseStatus = $"Parsed as flat {damageType} damage";
            return parseDamage;
        }

        // 2. Handle dice expressions with optional damage type (e.g., "2d6+3 slashing" or "1d4")
        // Added anchors ^ and $ to ensure it matches the entire string accurately
        var diceMatch = ParseDamage.DicePattern.Match(normalizedDamageExpression);
        
        if (diceMatch.Success)
        {
            int numberOfDice = string.IsNullOrEmpty(diceMatch.Groups[1].Value) ? 1 : int.Parse(diceMatch.Groups[1].Value);
            int sides = int.Parse(diceMatch.Groups[2].Value);
            int modifier = diceMatch.Groups[3].Success ? int.Parse(diceMatch.Groups[3].Value) : 0;
            string damageType = diceMatch.Groups[4].Success ? diceMatch.Groups[4].Value : "untyped";

            parseDamage.DamageDieCount = numberOfDice;
            parseDamage.DamageDieBase = sides;
            parseDamage.DamageModifier = modifier;
            parseDamage.AverageDamage = (numberOfDice * (sides + 1) / 2.0) + modifier;
            parseDamage.DamageType = damageType;
            parseDamage.ParseStatus = $"Parsed as {damageType} dice expression";
            return parseDamage;
        }

        // We can't parse it.
        parseDamage.ParseStatus = "Error: Invalid damage expression format";
        return parseDamage;
    }

    #endregion

}