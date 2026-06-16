using BattleReady.Core.Features.Calculator.Services;

namespace BattleReady.Tests.ParseDamage;

public class ParseDamageServiceTests
{

    // Use [Theory] when there are many cases of the same shape.
    // It's a tool for collapsing genuine repetition, not a default for every test class.
    
    private readonly ParseDamageService _service = new();

    // -------------------------------------------------------------------------
    // Empty / invalid input
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]        // Calculate_ReturnsError_WhenExpressionIsEmpty
    [InlineData("   ")]     // Calculate_ReturnsError_WhenExpressionIsWhitespace
    [InlineData("abc")]     // Calculate_ReturnsError_WhenExpressionIsInvalid
    public void Calculate_ReturnsError_ForInvalidExpressions(string expression)
    {
        var result = _service.Calculate(expression);
        Assert.Equal(0, result.AverageDamage);
        Assert.Contains("Error", result.ParseStatus);
    }

    // -------------------------------------------------------------------------
    // Flat damage expressions
    // -------------------------------------------------------------------------

    [Fact]
    public void Calculate_ParsesFlatDamage_NoType()
    {
        var result = _service.Calculate("5");
        Assert.Equal(5, result.AverageDamage);
        Assert.Equal(0, result.DamageDieCount);
        Assert.Equal(0, result.DamageDieBase);
        Assert.Equal(5, result.DamageModifier);
        Assert.Equal("untyped", result.DamageType);
    }

    [Fact]
    public void Calculate_ParsesFlatDamage_WithType()
    {
        var result = _service.Calculate("5 slashing");
        Assert.Equal(5, result.AverageDamage);
        Assert.Equal("slashing", result.DamageType);
    }

    [Fact]
    public void Calculate_ParsesFlatDamage_PreservesOriginalExpression()
    {
        var result = _service.Calculate("5 slashing");
        Assert.Equal("5 slashing", result.OriginalExpression);
    }

    // -------------------------------------------------------------------------
    // Dice expressions — average damage calculation
    // -------------------------------------------------------------------------

    [Fact]
    public void Calculate_ParsesDiceExpression_1d6()
    {
        // avg of 1d6 = (1+6)/2 = 3.5
        var result = _service.Calculate("1d6");
        Assert.Equal(3.5, result.AverageDamage);
        Assert.Equal(1, result.DamageDieCount);
        Assert.Equal(6, result.DamageDieBase);
        Assert.Equal(0, result.DamageModifier);
        Assert.Equal("untyped", result.DamageType);
    }

    [Fact]
    public void Calculate_ParsesDiceExpression_2d6Plus3()
    {
        // avg of 2d6 = 2 * (6+1)/2 = 7, plus 3 = 10
        var result = _service.Calculate("2d6+3");
        Assert.Equal(10, result.AverageDamage);
        Assert.Equal(2, result.DamageDieCount);
        Assert.Equal(6, result.DamageDieBase);
        Assert.Equal(3, result.DamageModifier);
    }

    [Fact]
    public void Calculate_ParsesDiceExpression_WithNegativeModifier()
    {
        // avg of 1d8 = (8+1)/2 = 4.5, minus 1 = 3.5
        var result = _service.Calculate("1d8-1");
        Assert.Equal(3.5, result.AverageDamage);
        Assert.Equal(-1, result.DamageModifier);
    }

    [Fact]
    public void Calculate_ParsesDiceExpression_WithDamageType()
    {
        var result = _service.Calculate("2d6+3 slashing");
        Assert.Equal(10, result.AverageDamage);
        Assert.Equal("slashing", result.DamageType);
    }

    [Fact]
    public void Calculate_ParsesDiceExpression_ImpliedOneDie()
    {
        // "d6" with no leading number should be treated as 1d6
        var result = _service.Calculate("d6");
        Assert.Equal(3.5, result.AverageDamage);
        Assert.Equal(1, result.DamageDieCount);
    }

    // -------------------------------------------------------------------------
    // Case and whitespace tolerance
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("2D6+3 SLASHING")]
    [InlineData("  2d6+3   slashing  ")]
    public void Calculate_NormalizesCaseAndWhitespace(string expression)
    {
        var result = _service.Calculate(expression);
        Assert.Equal(10, result.AverageDamage);
        Assert.Equal("slashing", result.DamageType);
    }
}