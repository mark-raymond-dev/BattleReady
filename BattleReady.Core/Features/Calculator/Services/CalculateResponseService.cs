namespace BattleReady.Features.Calculator.Services;

using BattleReady.Features.Calculator.Models;

public class CalculateResponseService
{
    
    #region Private Static Methods

    private static List<AttackInput> EnsureUniqueAttackNumbers(IEnumerable<AttackInput> attacks)
    {
        var result = new List<AttackInput>();
        var seen = new HashSet<int>();
        int counter = 1;
        foreach (var attack in attacks)
        {
            var copy = attack.Clone(); // shallow clone
            if (copy.AttackNumber < 1 || seen.Contains(copy.AttackNumber))
                copy.AttackNumber = counter;
            seen.Add(copy.AttackNumber);
            counter++;
            result.Add(copy);
        }
        return result;
    }

    private static double DeriveAltDamage(double avgDmgNormalHit, string altAvgDmgExpression, double defaultAltAvgDmg = 0)
    {
        var normalizedExpression = altAvgDmgExpression.Trim().ToLower();
        var halfKeywords = new[] { "half", "halved", "halve", "1/2", "50%" };
        var doubleKeywords = new[] { "double", "doubled", "dbl", "2x", "200%" };
        var tripleKeywords = new[] { "triple", "tripled", "trp", "3x", "300%" };
        var zeroKeywords = new[] { "0", "zero", "none" };

        bool isBlank = string.IsNullOrWhiteSpace(altAvgDmgExpression);
        bool isHalf = halfKeywords.Contains(normalizedExpression);
        bool isDouble = doubleKeywords.Contains(normalizedExpression);
        bool isTriple = tripleKeywords.Contains(normalizedExpression);
        bool isZero = zeroKeywords.Contains(normalizedExpression);
        
        if (isBlank) return defaultAltAvgDmg;        
        if (isHalf) return avgDmgNormalHit / 2;
        if (isDouble) return avgDmgNormalHit * 2;
        if (isTriple) return avgDmgNormalHit * 3;
        if (isZero) return 0;

        var parseDamageService = new ParseDamageService();
        var altDamageParseResult = parseDamageService.Calculate(altAvgDmgExpression);
        return altDamageParseResult.AverageDamage;
    }

    private static AttackInput ApplyDefaults(AttackInput defaultAttack, AttackInput attack)
    {
        var effectiveAttack = new AttackInput
        {
            AttackNumber = attack.AttackNumber,
            IsDefaultAttack = attack.IsDefaultAttack,
            
            BaseToHit = defaultAttack.BaseToHit,
            HasMAP = defaultAttack.HasMAP,
            IsAgile = defaultAttack.IsAgile,
            IsSpellRequiringSavingThrow = defaultAttack.IsSpellRequiringSavingThrow,

            NormalHitDamage = defaultAttack.NormalHitDamage,
            CritHitDamage = defaultAttack.CritHitDamage,
            NormalMissDamage = defaultAttack.NormalMissDamage,
            CritMissDamage = defaultAttack.CritMissDamage
        };

        return effectiveAttack;
    }

    #endregion

    #region Public Methods

    public CalculationResponse Calculate(CalculationRequest request)
    {
        var response = new CalculationResponse
        {
             OriginalRequest = request
        };

        // Ensure Attack Numbers are Unique, then sort attacks by AttackNumber.
        var sortedAttacks = EnsureUniqueAttackNumbers(request.Attacks)
                        .OrderBy(a => a.AttackNumber)
                        .ToList();

        var hitChanceService = new HitChanceService();
        var parseDamageService = new ParseDamageService();

        // Iterate through each attack and calculate results, building up the response as we go.
        foreach (var attack in sortedAttacks)
        {
            // Create effective attack, pulling in default values as needed.
            var effectiveAttack = attack.IsDefaultAttack ? ApplyDefaults(request.DefaultAttack!, attack) : attack;            

            // Start building out the response for this attack.
            var attackResult = new AttackResult
            {
                AttackNumber = effectiveAttack.AttackNumber
            };

            // Calculate effective to-hit and defense values.
            var effectiveToHit = effectiveAttack.BaseToHit;
            if (effectiveAttack.HasMAP)
            {
                var mapAdjustment = effectiveAttack.IsAgile ? -4 : -5;
                var attacksPastTheFirst = effectiveAttack.AttackNumber - 1;
                var totalMapAdjustment = mapAdjustment * attacksPastTheFirst;
                effectiveToHit += totalMapAdjustment;
            }
            attackResult.EffectiveToHit = effectiveToHit;
            attackResult.EffectiveDefense = request.EnemyDefense;

            // Calculate chances for each degree of success based on to-hit and defense.
            var hitChance = hitChanceService.Calculate(effectiveToHit, request.EnemyDefense, request.Natural20Upgrades, request.Natural1Downgrades);
            attackResult.CritHitChance = hitChance.CritHitChance;
            attackResult.NormalHitChance = hitChance.NormalHitChance;
            attackResult.NormalMissChance = hitChance.NormalMissChance;
            attackResult.CritMissChance = hitChance.CritMissChance;

            // Calculate average damage for each degree of success.
            var normalDamageParseResult = parseDamageService.Calculate(effectiveAttack.NormalHitDamage);
            attackResult.AvgDmgNormalHit = normalDamageParseResult.AverageDamage;
            attackResult.AvgDmgCritHit = DeriveAltDamage(attackResult.AvgDmgNormalHit, effectiveAttack.CritHitDamage, attackResult.AvgDmgNormalHit * 2);
            double defaultAvgDmgNormalMiss = effectiveAttack.IsSpellRequiringSavingThrow ? attackResult.AvgDmgNormalHit / 2 : 0;
            attackResult.AvgDmgNormalMiss = DeriveAltDamage(attackResult.AvgDmgNormalHit, effectiveAttack.NormalMissDamage, defaultAvgDmgNormalMiss);
            attackResult.AvgDmgCritMiss = DeriveAltDamage(attackResult.AvgDmgNormalHit, effectiveAttack.CritMissDamage, 0);

            // Calculate total expected damage, considering all degrees of success,
            // their average damage, and their chances. Add to grand total as well.
            attackResult.TotalExpectedDamage =
                (attackResult.CritHitChance * attackResult.AvgDmgCritHit) +
                (attackResult.NormalHitChance * attackResult.AvgDmgNormalHit) +
                (attackResult.NormalMissChance * attackResult.AvgDmgNormalMiss) +
                (attackResult.CritMissChance * attackResult.AvgDmgCritMiss);
            
            // Add this attack's results to the response.
            response.AttackResults.Add(attackResult);
        }

        // Calculate grand total expected damage for all attacks.
        response.TotalExpectedDamageAllAttacks = response.AttackResults.Sum(ar => ar.TotalExpectedDamage);

        return response;
    }

    #endregion

}