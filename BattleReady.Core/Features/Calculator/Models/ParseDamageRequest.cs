using System.ComponentModel.DataAnnotations;

namespace BattleReady.Features.Calculator.Models;

public class ParseDamageRequest
{

    #region Properties with DataAnnotations (e.g. Required or Validation)

    [Required(AllowEmptyStrings = false, ErrorMessage = "Expression is required.")]
    public string Expression { get; set; } = string.Empty;

    #endregion

}