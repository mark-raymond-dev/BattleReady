using System.ComponentModel.DataAnnotations;

namespace BattleReady.Api.Models.Requests;

public class ParseDamageRequest
{

    #region Properties with DataAnnotations (e.g. Required or Validation)

    [Required(AllowEmptyStrings = false, ErrorMessage = "Expression is required.")]
    public string Expression { get; set; } = string.Empty;

    #endregion

}