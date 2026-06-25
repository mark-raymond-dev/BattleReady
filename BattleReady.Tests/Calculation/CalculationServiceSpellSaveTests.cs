using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;
using Moq;

namespace BattleReady.Tests.Calculator;

public class CalculationServiceSpellSaveTests
{
    private readonly Mock<IHitChanceService>   _mockHitChanceService   = new();
    private readonly Mock<ISpellSaveService>   _mockSpellSaveService   = new();
    private readonly Mock<IParseDamageService> _mockParseDamageService = new();
    private readonly CalculationService _service;

    public CalculationServiceSpellSaveTests()
    {
        _service = new CalculationService(
            _mockHitChanceService.Object,
            _mockSpellSaveService.Object,
            _mockParseDamageService.Object);
    }

    // -----------------------------------------------------------------------
    // Routing: IsSpellRequiringSavingThrow = true → SpellSaveService
    // -----------------------------------------------------------------------

    [Fact]
    public void Calculate_SpellSaveAttack_CallsSpellSaveServiceNotHitChanceService()
    {
        // Arrange
        int saveBonus = 8;
        int spellDc   = 20;
        bool nat20    = true;
        bool nat1     = true;

        _mockSpellSaveService
            .Setup(s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(new SpellSaveResponse());

        _mockParseDamageService
            .Setup(s => s.Calculate(It.IsAny<string>()))
            .Returns(new ParseDamageResponse());

        var input = new CalculationInput
        {
            Natural20Upgrades  = nat20,
            Natural1Downgrades = nat1,
            Attacks = new List<AttackInput>
            {
                new SpellInput
                {
                    AttackNumber    = 1,
                    SkillRating     = saveBonus,
                    TargetScore     = spellDc,
                    NormalHitDamage = "4d6",
                }
            }
        };

        // Act
        _service.Calculate(input);

        // Assert — SpellSaveService called once with the save bonus and spell DC
        _mockSpellSaveService.Verify(
            s => s.Calculate(saveBonus, spellDc, nat20, nat1),
            Times.Once);

        // HitChanceService must NOT be called for a spell save attack
        _mockHitChanceService.Verify(
            s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public void Calculate_NormalAttack_CallsHitChanceServiceNotSpellSaveService()
    {
        // Arrange
        _mockHitChanceService
            .Setup(s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(new HitChanceResponse());

        _mockParseDamageService
            .Setup(s => s.Calculate(It.IsAny<string>()))
            .Returns(new ParseDamageResponse());

        var input = new CalculationInput
        {
            Natural20Upgrades  = true,
            Natural1Downgrades = true,
            Attacks = new List<AttackInput>
            {
                new AttackInput
                {
                    AttackNumber    = 1,
                    SkillRating     = 10,
                    TargetScore     = 18,
                    NormalHitDamage = "2d6+4",
                    // IsSpellRequiringSavingThrow defaults to false
                }
            }
        };

        // Act
        _service.Calculate(input);

        // Assert — HitChanceService called, SpellSaveService not
        _mockHitChanceService.Verify(
            s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Once);

        _mockSpellSaveService.Verify(
            s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Never);
    }

    // -----------------------------------------------------------------------
    // SpellSaveService results flow correctly into AttackResponse
    // -----------------------------------------------------------------------

    [Fact]
    public void Calculate_SpellSaveAttack_ChancesFromSpellSaveServicePopulateAttackResponse()
    {
        // Arrange
        double critHitChance    = 0.10;
        double normalHitChance  = 0.40;
        double normalMissChance = 0.35;
        double critMissChance   = 0.15;

        double normalHitDamage = 14.0;  // from ParseDamageService stub

        // (0.10 × 28.0) + (0.40 × 14.0) + (0.35 × 7.0) + (0.15 × 0.0)
        // = 2.80 + 5.60 + 2.45 + 0.0 = 10.85
        double totalExpectedDamage = 10.85;

        _mockSpellSaveService
            .Setup(s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(new SpellSaveResponse
            {
                CritHitChance    = critHitChance,
                NormalHitChance  = normalHitChance,
                NormalMissChance = normalMissChance,
                CritMissChance   = critMissChance,
            });

        _mockParseDamageService
            .Setup(s => s.Calculate(It.IsAny<string>()))
            .Returns(new ParseDamageResponse { AverageDamage = normalHitDamage });

        var input = new CalculationInput
        {
            Natural20Upgrades  = true,
            Natural1Downgrades = true,
            Attacks = new List<AttackInput>
            {
                new SpellInput
                {
                    AttackNumber    = 1,
                    SkillRating     = 6,
                    TargetScore     = 22,
                    NormalHitDamage = "4d6",
                    // CritHitDamage blank → default is double (28.0)
                    // NormalMissDamage blank → default for spells is half (7.0)
                    // CritMissDamage blank → default is 0
                }
            }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        var attack = result.AttackResponses.Single();
        Assert.Equal(critHitChance,    attack.CritHitChance);
        Assert.Equal(normalHitChance,  attack.NormalHitChance);
        Assert.Equal(normalMissChance, attack.NormalMissChance);
        Assert.Equal(critMissChance,   attack.CritMissChance);
        Assert.Equal(totalExpectedDamage, attack.TotalExpectedDamage, precision: 10);
    }

    // -----------------------------------------------------------------------
    // MAP is NOT applied to spell save attacks (HasMAP = false on SpellInput)
    // -----------------------------------------------------------------------

    [Fact]
    public void Calculate_SpellSaveAttack_DoesNotApplyMAP()
    {
        // Arrange — a SpellInput with AttackNumber = 3.
        // If MAP were applied it would subtract 10 from SkillRating.
        // We verify SpellSaveService is called with the raw saveBonus, not reduced.
        int saveBonus = 5;
        int spellDc   = 20;

        _mockSpellSaveService
            .Setup(s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(new SpellSaveResponse());

        _mockParseDamageService
            .Setup(s => s.Calculate(It.IsAny<string>()))
            .Returns(new ParseDamageResponse());

        var input = new CalculationInput
        {
            Natural20Upgrades  = true,
            Natural1Downgrades = true,
            Attacks = new List<AttackInput>
            {
                new SpellInput
                {
                    AttackNumber    = 3,    // would trigger -10 MAP penalty on a normal attack
                    SkillRating     = saveBonus,
                    TargetScore     = spellDc,
                    NormalHitDamage = "4d6",
                }
            }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert — EffectiveToHit equals the raw saveBonus, no MAP deduction
        var attack = result.AttackResponses.Single();
        Assert.Equal(saveBonus, attack.EffectiveSkillRating);

        // SpellSaveService was called with the original saveBonus, not saveBonus - 10
        _mockSpellSaveService.Verify(
            s => s.Calculate(saveBonus, spellDc, It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // Mixed turn: some attacks, some spells — each routes to the right service
    // -----------------------------------------------------------------------

    [Fact]
    public void Calculate_MixedTurn_RoutesEachAttackToCorrectService()
    {
        // Arrange — one normal attack and one spell save in the same turn
        _mockHitChanceService
            .Setup(s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(new HitChanceResponse());

        _mockSpellSaveService
            .Setup(s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(new SpellSaveResponse());

        _mockParseDamageService
            .Setup(s => s.Calculate(It.IsAny<string>()))
            .Returns(new ParseDamageResponse());

        var input = new CalculationInput
        {
            Natural20Upgrades  = true,
            Natural1Downgrades = true,
            Attacks = new List<AttackInput>
            {
                new AttackInput
                {
                    AttackNumber    = 1,
                    SkillRating     = 10,
                    TargetScore     = 18,
                    HasMAP          = true,
                    NormalHitDamage = "2d6+4",
                },
                new SpellInput
                {
                    AttackNumber    = 2,
                    SkillRating     = 6,
                    TargetScore     = 22,
                    NormalHitDamage = "4d6",
                }
            }
        };

        // Act
        _service.Calculate(input);

        // Assert — one call each, routed to the right service
        _mockHitChanceService.Verify(
            s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Exactly(1));

        _mockSpellSaveService.Verify(
            s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Exactly(1));
    }
}
