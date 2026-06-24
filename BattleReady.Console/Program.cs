using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;

var input = new CalculationInput
{
    CharacterName = "Corrupted Wildfire",
    Natural20Upgrades = false,
    Natural1Downgrades = false,
    DefaultAttack = new AttackInput { SkillRating = 12, TargetScore = 19, NormalHitDamage = "1d6+6 fire", CritHitDamage = "dbl", NormalMissDamage = "0", CritMissDamage = "0" },
    Attacks = [
        new AttackInput { AttackNumber = 1, IsDefaultAttack = true },
        new AttackInput { AttackNumber = 2, IsDefaultAttack = true },
        new AttackInput { AttackNumber = 3, IsDefaultAttack = true },
    ]
};

// Create services for dependency injection, then instantiate our main calculation service.
var parseDamageService = new ParseDamageService();
var hitChanceService = new HitChanceService();
var calculationService = new CalculationService(hitChanceService, parseDamageService);

// Call service and print out response.
var response = calculationService.Calculate(input);
Console.WriteLine(response);
