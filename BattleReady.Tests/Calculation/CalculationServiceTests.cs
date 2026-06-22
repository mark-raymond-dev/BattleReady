using BattleReady.Core.Features.Calculator.Models;
using BattleReady.Core.Features.Calculator.Services;
using Moq;

namespace BattleReady.Tests.Calculator;

public class CalculationServiceTests
{
    private readonly Mock<IHitChanceService> _mockHitChanceService = new();
    private readonly Mock<IParseDamageService> _mockParseDamageService = new();
    private readonly CalculationService _service;

    public CalculationServiceTests()
    {
        _service = new CalculationService(
            _mockHitChanceService.Object,
            _mockParseDamageService.Object);
    }

    [Fact]
    public void Calculate_SingleAttack_UsesHitChanceAndDamageFromDependencies()
    {
        //-----------------
        // ARRANGE
        //-----------------

        // Set our initial test data values.

        // Define values for CalculationInput.
        int enemyDefense = 15;
        bool nat20Upgrade = true;
        bool nat1Downgrade = true;

        // Define values for CalculationInput's first (and only) attack.
        int baseToHit = 8;
        string normHitDiceExp = "2d6+3";
        string critHitDiceExp = "double";
        string normMissDiceExp = "half";
        string critMissDiceExp = "zero";
        bool isSpellReqSavingThrow = true;

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

        // Stub ParseDamageService dependency to return known, fixed values
        _mockParseDamageService
            .Setup(s => s.Calculate(It.IsAny<string>()))
            .Returns(new ParseDamageResponse
            {
                AverageDamage = normHitDamage,
                ParseStatus = parseStatus
            });

        // Setup direct input object for CalculationService
        var input = new CalculationInput
        {
            EnemyDefense = enemyDefense,
            Natural20Upgrades = nat20Upgrade,
            Natural1Downgrades = nat1Downgrade,
            Attacks = new List<AttackInput>
            {
                new AttackInput
                {
                    AttackNumber = 1,
                    BaseToHit = baseToHit,
                    NormalHitDamage = normHitDiceExp,
                    CritHitDamage = critHitDiceExp,
                    NormalMissDamage = normMissDiceExp,
                    CritMissDamage = critMissDiceExp,
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
        Assert.Equal(normHitDamage, attack.AvgDmgNormalHit);        // came straight from the mocked ParseDamageResponse
        Assert.Equal(critHitDamage, attack.AvgDmgCritHit);          // "double" keyword = 2.0 x AvgDmgNormalHit, no service call needed
        Assert.Equal(normMissDamage, attack.AvgDmgNormalMiss);      // "half" keyword   = 0.5 x AvgDmgNormalHit, no service call needed
        Assert.Equal(critMissDamage, attack.AvgDmgCritMiss);        // "zero" keyword   = 0.0 x AvgDmgNormalHit, no service call needed

        // Attack Response: Liklihood of each of the 4 degrees of success
        Assert.Equal(critHitChance, attack.CritHitChance);
        Assert.Equal(normHitChance, attack.NormalHitChance);
        Assert.Equal(normMissChance, attack.NormalMissChance);
        Assert.Equal(critMissChance, attack.CritMissChance);

        // Attack Reponse: Statistical average damage expected
        Assert.Equal(totalExpectedDamage, attack.TotalExpectedDamage);

        // Verify the dependency (HitChanceService) was actually called
        // the right number of times, with the right to-hit/defense values.
        _mockHitChanceService.Verify(
            s => s.Calculate(baseToHit, enemyDefense, nat20Upgrade, nat1Downgrade),
            Times.Once);

        // Verify the dependency (ParseDamageService) was actually called
        // the right number of times, with the right dice expression.
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

        // Set our initial test data values.

        // Define values for CalculationInput.
        int enemyDefense = 15;
        bool nat20Upgrade = true;
        bool nat1Downgrade = true;

        // Define values for CalculationInput's first (and only) attack.
        // Note: normHitDiceExp, critHitDiceExp, normMissDiceExp, critMissDiceExp, and isSpellReqSavingThrow
        // below are NOT asserted on in this test — they exist only to satisfy AttackInput's shape.
        // This test only verifies HitChanceService call counts and to-hit values (see ASSERT section).
        bool hasMap = true;
        bool isAgile = false;
        int baseToHit = 10;
        string normHitDiceExp = "2d6+3";        // Not asserted - can be anything
        string critHitDiceExp = "double";       // Not asserted - can be anything
        string normMissDiceExp = "half";        // Not asserted - can be anything
        string critMissDiceExp = "zero";        // Not asserted - can be anything
        bool isSpellReqSavingThrow = true;      // Not asserted - can be anything

        // Define values for expected pass-in values for the 2nd and 3rd attacks.
        int secondAttackToHit = 5;  // has MAP, but is not Agile ... means 10 - 5 = 5
        int thirdAttackToHit = 0;   // has MAP, but is not Agile ... means 10 - 5 - 5 = 0

        // Stub HitChanceService dependency to return known, fixed values
        // (actually in this case we don't care, it doesn't matter)
        _mockHitChanceService
            .Setup(s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(new HitChanceResponse());

        // Stub ParseDamageService dependency to return known, fixed values
        // (actually in this case we don't care, it doesn't matter)
        _mockParseDamageService
            .Setup(s => s.Calculate(It.IsAny<string>()))
            .Returns(new ParseDamageResponse());

        // Setup direct input object for CalculationService.
        // Note: normHitDiceExp, critHitDiceExp, normMissDiceExp, critMissDiceExp, and isSpellReqSavingThrow
        // below are NOT asserted on in this test — they exist only to satisfy AttackInput's shape.
        // This test only verifies HitChanceService call counts and to-hit values (see ASSERT section).
        var input = new CalculationInput
        {
            EnemyDefense = enemyDefense,
            Natural20Upgrades = nat20Upgrade,
            Natural1Downgrades = nat1Downgrade,
            Attacks = new List<AttackInput>
            {
                new AttackInput
                {
                    AttackNumber = 1,
                    HasMAP = hasMap,
                    IsAgile = isAgile,
                    BaseToHit = baseToHit,
                    NormalHitDamage = normHitDiceExp,
                    CritHitDamage = critHitDiceExp, 
                    NormalMissDamage = normMissDiceExp,
                    CritMissDamage = critMissDiceExp,
                    IsSpellRequiringSavingThrow = isSpellReqSavingThrow
                },
                new AttackInput
                {
                    AttackNumber = 2,
                    HasMAP = hasMap,
                    IsAgile = isAgile,
                    BaseToHit = baseToHit,
                    NormalHitDamage = normHitDiceExp,
                    CritHitDamage = critHitDiceExp, 
                    NormalMissDamage = normMissDiceExp,
                    CritMissDamage = critMissDiceExp,
                    IsSpellRequiringSavingThrow = isSpellReqSavingThrow
                },
                new AttackInput
                {
                    AttackNumber = 3,
                    HasMAP = hasMap,
                    IsAgile = isAgile,
                    BaseToHit = baseToHit,
                    NormalHitDamage = normHitDiceExp,
                    CritHitDamage = critHitDiceExp, 
                    NormalMissDamage = normMissDiceExp,
                    CritMissDamage = critMissDiceExp,
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
            s => s.Calculate(baseToHit, enemyDefense, nat20Upgrade, nat1Downgrade),
            Times.Exactly(1));

        _mockHitChanceService.Verify(
            s => s.Calculate(secondAttackToHit, enemyDefense, nat20Upgrade, nat1Downgrade),
            Times.Exactly(1));

        _mockHitChanceService.Verify(
            s => s.Calculate(thirdAttackToHit, enemyDefense, nat20Upgrade, nat1Downgrade),
            Times.Exactly(1));
    }

    [Fact]
    public void Calculate_SingleAttack_CallsParseDamageServiceForEachNonKeywordExpression()
    {
        //-----------------
        // ARRANGE
        //-----------------

        // Set our initial test data values.

        // Define values for CalculationInput.
        int enemyDefense = 15;
        bool nat20Upgrade = true;
        bool nat1Downgrade = true;

        // Define values for CalculationInput's first (and only) attack.
        int baseToHit = 10;
        string normHitDiceExp = "2d6+4";
        string critHitDiceExp = "4d6+5";
        string normMissDiceExp = "1d6+2";
        string critMissDiceExp = "1d4";
        bool isSpellReqSavingThrow = true;

        // Stub HitChanceService dependency to return known, fixed values
        // (actually in this case we don't care, it doesn't matter)
        _mockHitChanceService
            .Setup(s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(new HitChanceResponse());

        // Stub ParseDamageService dependency to return known, fixed values
        // (actually in this case we don't care, it doesn't matter)
        _mockParseDamageService
            .Setup(s => s.Calculate(It.IsAny<string>()))
            .Returns(new ParseDamageResponse());

        // Setup direct input object for CalculationService
        var input = new CalculationInput
        {
            EnemyDefense = enemyDefense,
            Natural20Upgrades = nat20Upgrade,
            Natural1Downgrades = nat1Downgrade,
            Attacks = new List<AttackInput>
            {
                new AttackInput
                {
                    AttackNumber = 1,
                    BaseToHit = baseToHit,
                    NormalHitDamage = normHitDiceExp,
                    CritHitDamage = critHitDiceExp, 
                    NormalMissDamage = normMissDiceExp,
                    CritMissDamage = critMissDiceExp,
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

        // Both attacks have AttackNumber = 1 — EnsureUniqueAttackNumbers should
        // renumber the second one to 2, so MAP applies correctly.
        int duplicateAttackNumber = 1;
        int baseToHit = 10;
        string normHitDiceExp = "1d6";

        // With sequential numbering (1 and 2), MAP penalty on attack 2 = -5.
        // expectedSecondToHit = 10 + (-5 * (2-1)) = 5
        int expectedSecondToHit = 5;

        _mockHitChanceService
            .Setup(s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(new HitChanceResponse());

        _mockParseDamageService
            .Setup(s => s.Calculate(It.IsAny<string>()))
            .Returns(new ParseDamageResponse());

        var input = new CalculationInput
        {
            EnemyDefense = 15,
            Natural20Upgrades = true,
            Natural1Downgrades = true,
            Attacks = new List<AttackInput>
            {
                new AttackInput
                {
                    AttackNumber      = duplicateAttackNumber,
                    BaseToHit         = baseToHit,
                    HasMAP            = true,
                    IsAgile           = false,
                    NormalHitDamage   = normHitDiceExp,
                    CritHitDamage     = "double",
                    NormalMissDamage  = "0",
                    CritMissDamage    = "0"
                },
                new AttackInput
                {
                    AttackNumber      = duplicateAttackNumber,   // intentional duplicate
                    BaseToHit         = baseToHit,
                    HasMAP            = true,
                    IsAgile           = false,
                    NormalHitDamage   = normHitDiceExp,
                    CritHitDamage     = "double",
                    NormalMissDamage  = "0",
                    CritMissDamage    = "0"
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

        // If renumbering worked correctly, the second attack's effective to-hit
        // will reflect MAP penalty from AttackNumber 2 (i.e. baseToHit - 5 = 5).
        // If renumbering did NOT work, both attacks would be treated as AttackNumber 1
        // and no MAP penalty would be applied to either — HitChanceService would be
        // called twice with baseToHit (10) and never with expectedSecondToHit (5).
        _mockHitChanceService.Verify(
            s => s.Calculate(baseToHit, It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Exactly(1));

        _mockHitChanceService.Verify(
            s => s.Calculate(expectedSecondToHit, It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Exactly(1));
    }

    [Fact]
    public void Calculate_AttackWithIsDefaultAttack_UsesPropertiesFromDefaultAttack()
    {
        //----------------
        // ARRANGE
        //----------------

        // The attack itself has placeholder damage — the default should overwrite it.
        string placeholderDamage    = "1d4";        // on the attack — should NOT be used
        string defaultNormHitDamage = "2d6+5";      // on the default — should be used

        _mockHitChanceService
            .Setup(s => s.Calculate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(new HitChanceResponse());

        _mockParseDamageService
            .Setup(s => s.Calculate(It.IsAny<string>()))
            .Returns(new ParseDamageResponse());

        var input = new CalculationInput
        {
            EnemyDefense       = 15,
            Natural20Upgrades  = true,
            Natural1Downgrades = true,
            DefaultAttack = new AttackInput
            {
                AttackNumber     = 0,
                BaseToHit        = 12,
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
                    IsDefaultAttack  = true,            // triggers ApplyDefaults
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

        // ApplyDefaults should have caused ParseDamageService to be called with
        // the default's damage expression, not the attack's placeholder.
        _mockParseDamageService.Verify(
            s => s.Calculate(defaultNormHitDamage),
            Times.Exactly(1));

        _mockParseDamageService.Verify(
            s => s.Calculate(placeholderDamage),
            Times.Never());
    }
}