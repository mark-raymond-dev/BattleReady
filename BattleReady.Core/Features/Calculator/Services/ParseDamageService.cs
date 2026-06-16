namespace BattleReady.Core.Features.Calculator.Services;

using System.Text.RegularExpressions;
using BattleReady.Core.Features.Calculator.Models;

public class ParseDamageService : IParseDamageService
{

    #region RegEx Patterns

    private static readonly Regex FlatPattern = new(@"^(\d+)(?:\s+([a-zA-Z]+))?$", RegexOptions.Compiled);
    private static readonly Regex DicePattern = new(@"^(\d*)d(\d+)([+-]\d+)?(?:\s+([a-zA-Z]+))?$", RegexOptions.Compiled);

    #endregion

    #region Methods

    public ParseDamageResponse Calculate(string damageExpression)
    {
        var response = new ParseDamageResponse
        {
            OriginalExpression = damageExpression,
            AverageDamage = 0,
            ParseStatus = "Unparsed"
        };

        if (string.IsNullOrWhiteSpace(damageExpression))
        {
            response.ParseStatus = "Error: Empty damage expression";
            return response;
        }

        // Clean up casing and extra spaces, but KEEP single spaces between words
        var normalizedDamageExpression = Regex.Replace(damageExpression.Trim().ToLower(), @"\s+", " ");

        // 1. Handle flat damage with optional damage type (e.g., "5" or "5 slashing")
        var flatMatch = FlatPattern.Match(normalizedDamageExpression);
        
        if (flatMatch.Success)
        {
            int flatDamage = int.Parse(flatMatch.Groups[1].Value);
            string damageType = flatMatch.Groups[2].Success ? flatMatch.Groups[2].Value : "untyped";

            response.DamageDieCount = 0;
            response.DamageDieBase = 0;
            response.DamageModifier = flatDamage;
            response.DamageType = damageType;
            response.AverageDamage = flatDamage;
            response.ParseStatus = $"Parsed as flat {damageType} damage";
            return response;
        }

        // 2. Handle dice expressions with optional damage type (e.g., "2d6+3 slashing" or "1d4")
        // Added anchors ^ and $ to ensure it matches the entire string accurately
        var diceMatch = DicePattern.Match(normalizedDamageExpression);
        
        if (diceMatch.Success)
        {
            int numberOfDice = string.IsNullOrEmpty(diceMatch.Groups[1].Value) ? 1 : int.Parse(diceMatch.Groups[1].Value);
            int sides = int.Parse(diceMatch.Groups[2].Value);
            int modifier = diceMatch.Groups[3].Success ? int.Parse(diceMatch.Groups[3].Value) : 0;
            string damageType = diceMatch.Groups[4].Success ? diceMatch.Groups[4].Value : "untyped";

            response.DamageDieCount = numberOfDice;
            response.DamageDieBase = sides;
            response.DamageModifier = modifier;
            response.AverageDamage = (numberOfDice * (sides + 1) / 2.0) + modifier;
            response.DamageType = damageType;
            response.ParseStatus = $"Parsed as {damageType} dice expression";
            return response;
        }

        // We can't parse it.
        response.ParseStatus = "Error: Invalid damage expression format";
        return response;
    }

    #endregion

}