using System.ComponentModel.DataAnnotations;

namespace BattleReady.Features.Calculator.Models;

public class HitChanceRequest
{

    #region Properties with DataAnnotations (e.g. Required or Validation)

    // IMPORANT NOTE: When the JSON deserializer reads a request body
    // and a field like ToHit is missing entirely, it doesn't produce null.
    // It produces 0, because 0 is the default value of int.
    // If it is important to distinguish between a 0 generated from an omitted
    // value, and a 0 purposefully sent, we can make it a nullable data type
    // (i.e. "int?" instead of "int") and add [Required].

    [Required]
    [Range(-20, 50, ErrorMessage ="ToHit must be between -20 and 50.")]
    public int? ToHit { get; set; }
    
    [Range(1, 100, ErrorMessage = "Defense must be between 1 and 100.")]
    public int Defense { get; set; }

    #endregion

    #region Properties without DataAnnotations

    public bool Natural20Upgrades { get; set; } = true;
    public bool Natural1Downgrades { get; set; } = true;

    #endregion

}