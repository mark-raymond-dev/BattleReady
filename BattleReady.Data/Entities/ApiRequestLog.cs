using System.ComponentModel.DataAnnotations;

namespace BattleReady.Data.Entities;

public class ApiRequestLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string Endpoint { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string RequestBody { get; set; } = string.Empty;

    // ResponseBody is intentionally left as nvarchar(max).
    // nvarchar(n) is bounded to n=4000 characters maximum in SQL Server;
    // values above that threshold map back to nvarchar(max) anyway.
    // Response bodies for large attack sequences can exceed 4000 characters,
    // so truncating here would produce incomplete log records.
    public string ResponseBody { get; set; } = string.Empty;

    public int ResponseStatus { get; set; }
}