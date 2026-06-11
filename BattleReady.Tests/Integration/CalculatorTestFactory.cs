namespace BattleReady.Tests.Integration;

// This is to provide each test class (or each test) its own 
// uniquely-named in-memory database, making isolation explicit.

public class CalculatorTestFactory : IntegrationTestFactory
{
    public CalculatorTestFactory() : base("CalculatorTestDb") { }
}