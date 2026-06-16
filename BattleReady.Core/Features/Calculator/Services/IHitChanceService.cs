namespace BattleReady.Core.Features.Calculator.Services;

using BattleReady.Core.Features.Calculator.Models;

public interface IHitChanceService
{
    HitChanceResponse Calculate(int toHit, int defense, bool natural20Upgrades = true, bool natural1Downgrades = true);
}