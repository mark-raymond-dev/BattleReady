using BattleReady.Features.Calculator.Models;
using BattleReady.Features.Calculator.Services;

var request = new CalculationRequest
{
    CharacterName = "Corrupted Wildfire",
    EnemyDefense = 19,
    Natural20Upgrades = false,
    Natural1Downgrades = false,
    DefaultAttack = new AttackInput { BaseToHit = 12, NormalHitDamage = "1d6+6 fire", CritHitDamage = "dbl", NormalMissDamage = "0", CritMissDamage = "0" },
    Attacks = [
        new AttackInput { AttackNumber = 1, IsDefaultAttack = true },
        new AttackInput { AttackNumber = 2, IsDefaultAttack = true },
        new AttackInput { AttackNumber = 3, IsDefaultAttack = true },
    ]
};

// Create services for dependency injection, then instantiate our main calculation service.
var parseDamageService = new ParseDamageService();
var hitChanceService = new HitChanceService();
var calculateResponseService = new CalculateResponseService(hitChanceService, parseDamageService);

// Call service and print out response.
var response = calculateResponseService.Calculate(request);
Console.WriteLine(response);
