namespace BattleReady.Core.Features.Calculator.Services;

using BattleReady.Core.Features.Calculator.Models;

public class SpellSaveService : ISpellSaveService
{

    #region Injected Services

    private readonly IDegreeOfSuccessService _degreeOfSuccessService;

    #endregion

    #region Constructor

    public SpellSaveService(IDegreeOfSuccessService degreeOfSuccessService)
    {
        _degreeOfSuccessService = degreeOfSuccessService;
    }

    #endregion

    #region Methods

    public SpellSaveResponse Calculate(
        int saveBonus,
        int spellDc,
        bool natural20Upgrades  = true,
        bool natural1Downgrades = true)
    {
        // The defender rolls saveBonus + d20 against the caster's spellDc.
        // A high defender roll is GOOD for the defender and BAD for the caster,
        // so we invert the degree-of-success buckets when mapping to the caster's view.
        var result = _degreeOfSuccessService.Calculate(
            skillRating:        saveBonus,
            targetScore:        spellDc,
            natural20Upgrades:  natural20Upgrades,
            natural1Downgrades: natural1Downgrades);

        return new SpellSaveResponse
        {
            // Defender critically succeeded → caster critically missed (no damage)
            CritMissChance   = result.CritSuccessChance,
            // Defender succeeded → caster missed (partial damage, typically half)
            NormalMissChance = result.NormalSuccessChance,
            // Defender failed → caster hit (full damage)
            NormalHitChance  = result.NormalFailChance,
            // Defender critically failed → caster critically hit (bonus damage)
            CritHitChance    = result.CritFailChance,
        };
    }

    #endregion

}