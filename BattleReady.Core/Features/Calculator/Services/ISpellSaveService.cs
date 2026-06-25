namespace BattleReady.Core.Features.Calculator.Services;

using BattleReady.Core.Features.Calculator.Models;

public interface ISpellSaveService
{
    SpellSaveResponse Calculate(
        int saveBonus,
        int spellDc,
        bool natural20Upgrades  = true,
        bool natural1Downgrades = true);
}