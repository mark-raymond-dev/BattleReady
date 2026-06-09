namespace BattleReady.Data.Entities;

public class ApiRequestLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Endpoint { get; set; } = string.Empty;
    public string RequestBody { get; set; } = string.Empty;
    public int ResponseStatus { get; set; }
}
