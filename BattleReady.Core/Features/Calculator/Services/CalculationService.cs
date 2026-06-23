namespace BattleReady.Core.Features.Calculator.Services;

using BattleReady.Core.Features.Calculator.Models;

public class CalculationService : ICalculationService
{

    #region Injected Services

    private readonly IHitChanceService _hitChanceService;
    private readonly IParseDamageService _parseDamageService;

    #endregion

    #region Constructor

    public CalculationService(IHitChanceService hitChanceService, IParseDamageService parseDamageService)
    {
        _hitChanceService = hitChanceService;
        _parseDamageService = parseDamageService;
    }

    #endregion

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

    private static AttackInput ApplyDefaults(AttackInput defaultAttack, AttackInput attack)
    {
        // NOTE: This intentionally copies over (thus overwriting) all properties
        // from the passed in default except for AttackNumber and IsDefaultAttack.
        var effectiveAttack = new AttackInput
        {
            // Get these from the original object.
            AttackNumber = attack.AttackNumber,
            IsDefaultAttack = attack.IsDefaultAttack,
            
            // Get these from the default.
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

    #region Private Methods

    private double DeriveAltDamage(double avgDmgNormalHit, string altAvgDmgExpression, double defaultAltAvgDmg = 0)
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

        var altDamageParseResult = _parseDamageService.Calculate(altAvgDmgExpression);
        return altDamageParseResult.AverageDamage;
    }

    #endregion

    #region Public Methods

    public CalculationResponse Calculate(CalculationInput input)
    {
        var response = new CalculationResponse();

        // Ensure Attack Numbers are Unique, then sort attacks by AttackNumber.
        var sortedAttacks = EnsureUniqueAttackNumbers(input.Attacks)
                        .OrderBy(a => a.AttackNumber)
                        .ToList();

        // Iterate through each attack and calculate results, building up the response as we go.
        foreach (var attack in sortedAttacks)
        {
            // Create effective attack, pulling in default values as needed.
            var effectiveAttack = attack.IsDefaultAttack ? ApplyDefaults(input.DefaultAttack!, attack) : attack;            

            // Start building out the response for this attack.
            var attackResponse = new AttackResponse
            {
                AttackNumber = effectiveAttack.AttackNumber
            };

            // Calculate effective to-hit and defense values.
            var effectiveToHit = effectiveAttack.BaseToHit;
            if (effectiveAttack.HasMAP)
            {
                // Normal attack penalty is -5, unless you are using
                // an agile weapon, in which case it is -4.
                var mapAdjustment = effectiveAttack.IsAgile ? -4 : -5;

                // MAP penalty caps at the third attack in Pathfinder 2e — attacks 3, 4, 5, etc.
                // all use the same penalty as attack 3 (i.e. 2 tiers past the first).
                var attacksPastTheFirst = Math.Min(effectiveAttack.AttackNumber - 1, 2);

                // Determine the TOTAL penalty for this attack.
                var totalMapAdjustment = mapAdjustment * attacksPastTheFirst;
                effectiveToHit += totalMapAdjustment;
            }
            attackResponse.EffectiveToHit = effectiveToHit;
            attackResponse.EffectiveDefense = input.EnemyDefense;

            // Calculate chances for each degree of success based on to-hit and defense.
            var hitChance = _hitChanceService.Calculate(effectiveToHit, input.EnemyDefense, input.Natural20Upgrades, input.Natural1Downgrades);
            attackResponse.CritHitChance = hitChance.CritHitChance;
            attackResponse.NormalHitChance = hitChance.NormalHitChance;
            attackResponse.NormalMissChance = hitChance.NormalMissChance;
            attackResponse.CritMissChance = hitChance.CritMissChance;

            // Calculate average damage for each degree of success.
            var normalDamageParseResult = _parseDamageService.Calculate(effectiveAttack.NormalHitDamage);
            attackResponse.AvgDmgNormalHit = normalDamageParseResult.AverageDamage;
            attackResponse.AvgDmgCritHit = DeriveAltDamage(attackResponse.AvgDmgNormalHit, effectiveAttack.CritHitDamage, attackResponse.AvgDmgNormalHit * 2);
            double defaultAvgDmgNormalMiss = effectiveAttack.IsSpellRequiringSavingThrow ? attackResponse.AvgDmgNormalHit / 2 : 0;
            attackResponse.AvgDmgNormalMiss = DeriveAltDamage(attackResponse.AvgDmgNormalHit, effectiveAttack.NormalMissDamage, defaultAvgDmgNormalMiss);
            attackResponse.AvgDmgCritMiss = DeriveAltDamage(attackResponse.AvgDmgNormalHit, effectiveAttack.CritMissDamage, 0);

            // Calculate total expected damage, considering all degrees of success,
            // their average damage, and their chances. Add to grand total as well.
            attackResponse.TotalExpectedDamage =
                (attackResponse.CritHitChance * attackResponse.AvgDmgCritHit) +
                (attackResponse.NormalHitChance * attackResponse.AvgDmgNormalHit) +
                (attackResponse.NormalMissChance * attackResponse.AvgDmgNormalMiss) +
                (attackResponse.CritMissChance * attackResponse.AvgDmgCritMiss);
            
            // Add this attack's results to the response.
            response.AttackResponses.Add(attackResponse);
        }

        // Calculate grand total expected damage for all attacks.
        response.TotalExpectedDamageAllAttacks = response.AttackResponses.Sum(ar => ar.TotalExpectedDamage);

        return response;
    }

    #endregion

}