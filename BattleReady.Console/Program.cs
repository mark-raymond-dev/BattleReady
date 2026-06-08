using BattleReady.Features.Calculator.Models;
using BattleReady.Features.Calculator.Services;

var input = new CalculationInput
{
    CharacterName = "Corrupted Wildfire",
    EnemyDefense = 19,
    Natural20Upgrades = false,
    Natural1Downgrades = false,
    DefaultAttack = new AttackRequest { BaseToHit = 12, NormalHitDamage = "1d6+6 fire", CritHitDamage = "dbl", NormalMissDamage = "0", CritMissDamage = "0" },
    Attacks = [
        new AttackRequest { AttackNumber = 1, IsDefaultAttack = true },
        new AttackRequest { AttackNumber = 2, IsDefaultAttack = true },
        new AttackRequest { AttackNumber = 3, IsDefaultAttack = true },
    ]
};

// Create services for dependency injection, then instantiate our main calculation service.
var parseDamageService = new ParseDamageService();
var hitChanceService = new HitChanceService();
var calculateService = new CalculateService(hitChanceService, parseDamageService);

// Call service and print out response.
var response = calculateService.Calculate(input);
Console.WriteLine(response);
