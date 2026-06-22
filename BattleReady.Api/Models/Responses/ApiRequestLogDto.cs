namespace BattleReady.Api.Models.Responses;

public class ApiRequestLogDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string RequestBody { get; set; } = string.Empty;
    public string ResponseBody { get; set; } = string.Empty;
    public int ResponseStatus { get; set; }
}