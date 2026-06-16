namespace BattleReady.Core.Features.Calculator.Services;

using BattleReady.Core.Features.Calculator.Models;

public interface IParseDamageService
{
    ParseDamageResponse Calculate(string damageExpression);
}