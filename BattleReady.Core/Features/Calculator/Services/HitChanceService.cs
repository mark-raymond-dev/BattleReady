namespace BattleReady.Core.Features.Calculator.Services;

using BattleReady.Core.Features.Calculator.Models;

public class HitChanceService : IHitChanceService
{

    #region Injected Services

    private readonly IDegreeOfSuccessService _degreeOfSuccessService;

    #endregion

    #region Constructor

    public HitChanceService(IDegreeOfSuccessService degreeOfSuccessService)
    {
        _degreeOfSuccessService = degreeOfSuccessService;
    }

    #endregion

    #region Methods

    public HitChanceResponse Calculate(
        int toHit,
        int defense,
        bool natural20Upgrades  = true,
        bool natural1Downgrades = true)
    {
        var result = _degreeOfSuccessService.Calculate(
            skillRating:        toHit,
            targetScore:        defense,
            natural20Upgrades:  natural20Upgrades,
            natural1Downgrades: natural1Downgrades);

        return new HitChanceResponse
        {
            ToHit            = toHit,
            Defense          = defense,
            CritMissChance   = result.CritFailChance,
            NormalMissChance = result.NormalFailChance,
            NormalHitChance  = result.NormalSuccessChance,
            CritHitChance    = result.CritSuccessChance,
        };
    }

    #endregion

}