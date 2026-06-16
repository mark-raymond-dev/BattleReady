namespace BattleReady.Core.Features.Calculator.Services;

using BattleReady.Core.Features.Calculator.Models;

public interface ICalculationService
{
    CalculationResponse Calculate(CalculationInput input);
}