namespace BattleReady.Api.Models.Responses;

public class LogsResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public List<ApiRequestLogDto> Records { get; set; } = new();
}