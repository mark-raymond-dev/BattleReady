using System.ComponentModel.DataAnnotations;

namespace BattleReady.Api.Models.Requests;

public class LogsRequest
{

    #region Properties with DataAnnotations (e.g. Required or Validation)

    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;

    #endregion

    #region Properties without DataAnnotations

    public string? Endpoint { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    #endregion

}