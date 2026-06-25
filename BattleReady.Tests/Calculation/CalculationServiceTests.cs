using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;
using Moq;

namespace BattleReady.Tests.Calculator;

public class CalculationServiceTests
{
    private readonly Mock<IHitChanceService>   _mockHitChanceService   = new();
    private readonly Mock<ISpellSaveService>   _mockSpellSaveService   = new();
    private readonly Mock<IParseDamageService> _mockParseDamageService = new();
    private readonly CalculationService _service;

    public CalculationServiceTests()
    {
        _service = new CalculationService(
            _mockHitChanceService.Object,
            _mockSpellSaveService.Object,
            _mockParseDamageService.Object);
    }

    [Fact]
    public void Calculate_SingleAttack_UsesHitChanceAndDamageFromDependencies()
    {
        //-----------------
        // ARRANGE
        //-----------------

        // Define values for CalculationInput.
        int enemyDefense = 15;
        bool nat20Upgrade = true;
        bool nat1Downgrade = true;

        // Define values for CalculationInput's first (and only) attack.
        int skillRating = 8;
        string normHitDiceExp = "2d6+3";
        string critHitDiceExp = "double";
        string normMissDiceExp = "half";
        string critMissDiceExp = "zero";
        bool isSpellReqSavingThrow = false; // this test will be for a melee attack

        // Define what HitChanceService returns
        double critHitChance = 0.20;
        double normHitChance = 0.50;
        double normMissChance = 0.25;
        double critMissChance = 0.05;

        // Define what ParseDamageService returns
        double critHitDamage = 20.0;        // keyword lets us avoid service call
        double normHitDamage = 10.0;        // directly from service call
        double normMissDamage = 5.0;        // keyword lets us avoid service call
        double critMissDamage = 0.0;        // keyword lets us avoid service call
        string parseStatus = "Parsed as untyped dice expression";

        // Define values on AttackResponse
        // NOTE: Do not just copy/paste the formula here.
        // Do the math and hard code this value!
        // (0.20 × 20.0) + (0.50 × 10.0) + (0.25 × 5.0) + (0.05 × 0.0)
        // = (4.0) + (5.0) + (1.25) + (0.0) = 10.25
        double totalExpectedDamage = 10.25;

        // Stub HitChanceService dependency to return known, fixed values
        _mockHitChanceService
            .Setup(s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(new HitChanceResponse
            {
                CritHitChance = critHitChance,
                NormalHitChance = normHitChance,
                NormalMissChance = normMissChance,
                CritMissChance = critMissChance
            });
        
        _mockSpellSaveService
            .Setup(s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(new SpellSaveResponse());
        
        // Stub ParseDamageService dependency to return known, fixed values
        _mockParseDamageService
            .Setup(s => s.Calculate(It.IsAny<string>()))
            .Returns(new ParseDamageResponse
            {
                AverageDamage = normHitDamage,
                ParseStatus = parseStatus
            });

        var input = new CalculationInput
        {
            Natural20Upgrades  = nat20Upgrade,
            Natural1Downgrades = nat1Downgrade,
            Attacks = new List<AttackInput>
            {
                new AttackInput
                {
                    AttackNumber                = 1,
                    SkillRating                 = skillRating,
                    TargetScore                 = enemyDefense,
                    NormalHitDamage             = normHitDiceExp,
                    CritHitDamage               = critHitDiceExp,
                    NormalMissDamage            = normMissDiceExp,
                    CritMissDamage              = critMissDiceExp,
                    IsSpellRequiringSavingThrow = isSpellReqSavingThrow
                }
            }
        };


        //-----------------
        // ACT
        //-----------------

        var result = _service.Calculate(input);


        //-----------------
        // ASSERT
        //-----------------

        var attack = result.AttackResponses.Single();

        // Attack Response: Damage for each of the 4 degrees of success
        Assert.Equal(normHitDamage, attack.AvgDmgNormalHit);
        Assert.Equal(critHitDamage, attack.AvgDmgCritHit);
        Assert.Equal(normMissDamage, attack.AvgDmgNormalMiss);
        Assert.Equal(critMissDamage, attack.AvgDmgCritMiss);

        // Attack Response: Likelihood of each of the 4 degrees of success
        Assert.Equal(critHitChance, attack.CritHitChance);
        Assert.Equal(normHitChance, attack.NormalHitChance);
        Assert.Equal(normMissChance, attack.NormalMissChance);
        Assert.Equal(critMissChance, attack.CritMissChance);

        // Attack Response: Statistical average damage expected
        Assert.Equal(totalExpectedDamage, attack.TotalExpectedDamage);

        // Verify HitChanceService was called with the right values
        _mockHitChanceService.Verify(
            s => s.Calculate(skillRating, enemyDefense, nat20Upgrade, nat1Downgrade),
            Times.Once);

        // Verify ParseDamageService was called with the right expression
        _mockParseDamageService.Verify(
            s => s.Calculate(normHitDiceExp),
            Times.Once);
    }

    [Fact]
    public void Calculate_MultipleAttacks_CallsHitChanceServiceOncePerAttack()
    {
        //-----------------
        // ARRANGE
        //-----------------

        int enemyDefense = 15;
        bool nat20Upgrade = true;
        bool nat1Downgrade = true;

        bool hasMap = true;
        bool isAgile = false;
        int skillRating = 10;
        string normHitDiceExp = "2d6+3";
        string critHitDiceExp = "double";
        string normMissDiceExp = "half";
        string critMissDiceExp = "zero";
        bool isSpellReqSavingThrow = false; // this test will be for a melee attack

        // With sequential numbering (1, 2, 3), MAP penalties are -5 and -10.
        // secondAttackSkillRating = 10 + (-5 × 1) = 5
        // thirdAttackSkillRating  = 10 + (-5 × 2) = 0
        int secondAttackSkillRating = 5;
        int thirdAttackSkillRating  = 0;

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
            Natural20Upgrades  = nat20Upgrade,
            Natural1Downgrades = nat1Downgrade,
            Attacks = new List<AttackInput>
            {
                new AttackInput
                {
                    AttackNumber                = 1,
                    HasMAP                      = hasMap,
                    IsAgile                     = isAgile,
                    SkillRating                 = skillRating,
                    TargetScore                 = enemyDefense,
                    NormalHitDamage             = normHitDiceExp,
                    CritHitDamage               = critHitDiceExp,
                    NormalMissDamage            = normMissDiceExp,
                    CritMissDamage              = critMissDiceExp,
                    IsSpellRequiringSavingThrow = isSpellReqSavingThrow
                },
                new AttackInput
                {
                    AttackNumber                = 2,
                    HasMAP                      = hasMap,
                    IsAgile                     = isAgile,
                    SkillRating                 = skillRating,
                    TargetScore                 = enemyDefense,
                    NormalHitDamage             = normHitDiceExp,
                    CritHitDamage               = critHitDiceExp,
                    NormalMissDamage            = normMissDiceExp,
                    CritMissDamage              = critMissDiceExp,
                    IsSpellRequiringSavingThrow = isSpellReqSavingThrow
                },
                new AttackInput
                {
                    AttackNumber                = 3,
                    HasMAP                      = hasMap,
                    IsAgile                     = isAgile,
                    SkillRating                 = skillRating,
                    TargetScore                 = enemyDefense,
                    NormalHitDamage             = normHitDiceExp,
                    CritHitDamage               = critHitDiceExp,
                    NormalMissDamage            = normMissDiceExp,
                    CritMissDamage              = critMissDiceExp,
                    IsSpellRequiringSavingThrow = isSpellReqSavingThrow
                }
            }
        };


        //-----------------
        // ACT
        //-----------------

        _service.Calculate(input);


        //-----------------
        // ASSERT
        //-----------------

        _mockHitChanceService.Verify(
            s => s.Calculate(skillRating, enemyDefense, nat20Upgrade, nat1Downgrade),
            Times.Exactly(1));

        _mockHitChanceService.Verify(
            s => s.Calculate(secondAttackSkillRating, enemyDefense, nat20Upgrade, nat1Downgrade),
            Times.Exactly(1));

        _mockHitChanceService.Verify(
            s => s.Calculate(thirdAttackSkillRating, enemyDefense, nat20Upgrade, nat1Downgrade),
            Times.Exactly(1));
    }

    [Fact]
    public void Calculate_SingleAttack_CallsParseDamageServiceForEachNonKeywordExpression()
    {
        //-----------------
        // ARRANGE
        //-----------------

        int enemyDefense = 15;
        bool nat20Upgrade = true;
        bool nat1Downgrade = true;

        int skillRating = 10;
        string normHitDiceExp = "2d6+4";
        string critHitDiceExp = "4d6+5";
        string normMissDiceExp = "1d6+2";
        string critMissDiceExp = "1d4";
        bool isSpellReqSavingThrow = true;

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
            Natural20Upgrades  = nat20Upgrade,
            Natural1Downgrades = nat1Downgrade,
            Attacks = new List<AttackInput>
            {
                new AttackInput
                {
                    AttackNumber                = 1,
                    SkillRating                 = skillRating,
                    TargetScore                 = enemyDefense,
                    NormalHitDamage             = normHitDiceExp,
                    CritHitDamage               = critHitDiceExp,
                    NormalMissDamage            = normMissDiceExp,
                    CritMissDamage              = critMissDiceExp,
                    IsSpellRequiringSavingThrow = isSpellReqSavingThrow
                }
            }
        };


        //-----------------
        // ACT
        //-----------------

        _service.Calculate(input);


        //-----------------
        // ASSERT
        //-----------------

        _mockParseDamageService.Verify(
            s => s.Calculate(normHitDiceExp),
            Times.Exactly(1));

        _mockParseDamageService.Verify(
            s => s.Calculate(critHitDiceExp),
            Times.Exactly(1));

        _mockParseDamageService.Verify(
            s => s.Calculate(normMissDiceExp),
            Times.Exactly(1));

        _mockParseDamageService.Verify(
            s => s.Calculate(critMissDiceExp),
            Times.Exactly(1));
    }

    [Fact]
    public void Calculate_DuplicateAttackNumbers_AreRenumberedSequentially()
    {
        //----------------
        // ARRANGE
        //----------------

        int duplicateAttackNumber = 1;
        int skillRating = 10;
        int enemyDefense = 15;
        string normHitDiceExp = "1d6";

        // With sequential numbering (1 and 2), MAP penalty on attack 2 = -5.
        // expectedSecondSkillRating = 10 + (-5 × (2-1)) = 5
        int expectedSecondSkillRating = 5;

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
                    AttackNumber     = duplicateAttackNumber,
                    SkillRating      = skillRating,
                    TargetScore      = enemyDefense,
                    HasMAP           = true,
                    IsAgile          = false,
                    NormalHitDamage  = normHitDiceExp,
                    CritHitDamage    = "double",
                    NormalMissDamage = "0",
                    CritMissDamage   = "0"
                },
                new AttackInput
                {
                    AttackNumber     = duplicateAttackNumber,   // intentional duplicate
                    SkillRating      = skillRating,
                    TargetScore      = enemyDefense,
                    HasMAP           = true,
                    IsAgile          = false,
                    NormalHitDamage  = normHitDiceExp,
                    CritHitDamage    = "double",
                    NormalMissDamage = "0",
                    CritMissDamage   = "0"
                }
            }
        };

        //----------------
        // ACT
        //----------------

        _service.Calculate(input);

        //----------------
        // ASSERT
        //----------------

        _mockHitChanceService.Verify(
            s => s.Calculate(skillRating, It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Exactly(1));

        _mockHitChanceService.Verify(
            s => s.Calculate(expectedSecondSkillRating, It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Exactly(1));
    }

    [Fact]
    public void Calculate_AttackWithIsDefaultAttack_UsesPropertiesFromDefaultAttack()
    {
        //----------------
        // ARRANGE
        //----------------

        string placeholderDamage    = "1d4";
        string defaultNormHitDamage = "2d6+5";

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
            DefaultAttack = new AttackInput
            {
                AttackNumber     = 0,
                SkillRating      = 12,
                TargetScore      = 15,
                HasMAP           = true,
                IsAgile          = false,
                NormalHitDamage  = defaultNormHitDamage,
                CritHitDamage    = "double",
                NormalMissDamage = "0",
                CritMissDamage   = "0"
            },
            Attacks = new List<AttackInput>
            {
                new AttackInput
                {
                    AttackNumber     = 1,
                    IsDefaultAttack  = true,
                    NormalHitDamage  = placeholderDamage,
                    CritHitDamage    = "double",
                    NormalMissDamage = "0",
                    CritMissDamage   = "0"
                }
            }
        };

        //----------------
        // ACT
        //----------------

        _service.Calculate(input);

        //----------------
        // ASSERT
        //----------------

        _mockParseDamageService.Verify(
            s => s.Calculate(defaultNormHitDamage),
            Times.Exactly(1));

        _mockParseDamageService.Verify(
            s => s.Calculate(placeholderDamage),
            Times.Never());
    }

    [Theory]
    // Melee attacks that for some reason do not have MAP
    [InlineData(4, false, false, false, 10)]    // 4th attack, non-agile : no MAP means value stays the same → 10
    [InlineData(5, false, false, false, 10)]    // 5th attack, non-agile : no MAP means value stays the same → 10
    [InlineData(4, false, true,  false, 10)]    // 4th attack, agile : no MAP means value stays the same → 10
    [InlineData(5, false, true,  false, 10)]    // 5th attack, agile : no MAP means value stays the same → 10
    // "Spell Save" attacks that intrinsically can NEVER have MAP, even is we set hasMap = true
    [InlineData(4, true, false, true, 10)]      // 4th spell-save attack : no MAP means value stays the same → 10
    [InlineData(5, true, false, true, 10)]      // 5th spell-save attack : no MAP means value stays the same → 10
    [InlineData(4, true, true,  true, 10)]      // 4th spell-save attack : no MAP means value stays the same → 10
    [InlineData(5, true, true,  true, 10)]      // 5th spell-save attack : no MAP means value stays the same → 10    
    public void Calculate_MAPCapAtThirdAttack_EffectiveSkillRatingDoesNotContinueToDrop(
        int attackNumber, bool hasMap, bool isAgile, bool isSpellSave, int expectedEffectiveSkillRating)
    {
        // Arrange
        int skillRating  = 10;
        int enemyDefense = 20;

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
                    AttackNumber                = attackNumber,
                    SkillRating                 = skillRating,
                    TargetScore                 = enemyDefense,
                    HasMAP                      = hasMap,
                    IsAgile                     = isAgile,
                    IsSpellRequiringSavingThrow = isSpellSave,
                    NormalHitDamage             = "1d6",
                    CritHitDamage               = "",
                    NormalMissDamage            = "0",
                    CritMissDamage              = "0"
                }
            }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        var attackResult = result.AttackResponses.Single();
        Assert.Equal(expectedEffectiveSkillRating, attackResult.EffectiveSkillRating);
    }

    [Theory]
    [InlineData(false, 0)]   // non-agile: attacks 1,2,3,4 → cap at tier 2 → 10 + (-5 × 2) = 0
    [InlineData(true,  2)]   // agile:     attacks 1,2,3,4 → cap at tier 2 → 10 + (-4 × 2) = 2
    public void Calculate_MAPCap_AppliesAtThirdEligibleAttack(bool isAgile, int expectedEffectiveSkillRating)
    {
        // Arrange
        int skillRating  = 10;
        int enemyDefense = 20;

        _mockHitChanceService
            .Setup(s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(new HitChanceResponse());
        _mockSpellSaveService
            .Setup(s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(new SpellSaveResponse());
        _mockParseDamageService
            .Setup(s => s.Calculate(It.IsAny<string>()))
            .Returns(new ParseDamageResponse());

        // Build 4 sequential MAP-eligible attacks.
        // Attacks 3 AND 4 should both receive the same -10/-8 penalty (cap at tier 2).
        var attacks = Enumerable.Range(1, 4).Select(n => new AttackInput
        {
            AttackNumber    = n,
            SkillRating     = skillRating,
            TargetScore     = enemyDefense,
            HasMAP          = true,
            IsAgile         = isAgile,
            NormalHitDamage = "1d6",
            CritHitDamage   = "",
            NormalMissDamage = "0",
            CritMissDamage  = "0"
        }).ToList<AttackInput>();

        var input = new CalculationInput
        {
            Natural20Upgrades  = true,
            Natural1Downgrades = true,
            Attacks            = attacks
        };

        // Act
        var result = _service.Calculate(input);

        // Assert — attack 4 must match attack 3 (cap has held)
        // attack 3: tier 2 → 10 + penalty × 2
        // attack 4: tier 2 (capped, same as attack 3)
        int attack3SkillRating = result.AttackResponses.Single(a => a.AttackNumber == 3).EffectiveSkillRating;
        int attack4SkillRating = result.AttackResponses.Single(a => a.AttackNumber == 4).EffectiveSkillRating;

        Assert.Equal(expectedEffectiveSkillRating, attack3SkillRating);
        Assert.Equal(expectedEffectiveSkillRating, attack4SkillRating);   // cap: same as attack 3
        Assert.Equal(attack3SkillRating, attack4SkillRating);             // redundant but documents the cap intent
    }

}