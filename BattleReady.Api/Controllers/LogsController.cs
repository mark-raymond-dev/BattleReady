using BattleReady.Api.Models.Requests;
using BattleReady.Api.Models.Responses;
using BattleReady.Data;
using BattleReady.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BattleReady.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly AppDbContext _db;

    public LogsController(AppDbContext db)
    {
        _db = db;
    }

    // NOTE: Rule of thumb - any time you're awaiting something that itself accepts a 
    // CancellationToken overload, plumb it through. Database calls are the most common
    // offender in a typical web API, but it matters just as much if you swap Azure SQL
    // for MongoDB, or if an action called out to a third-party REST API.

    [HttpGet]
    public async Task<ActionResult<LogsResponse>> GetLogs(
        [FromQuery] LogsRequest request,
        CancellationToken cancellationToken)
    {
        var query = _db.ApiRequestLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Endpoint))
            query = query.Where(x => x.Endpoint.Contains(request.Endpoint));

        if (request.From.HasValue)
            query = query.Where(x => x.Timestamp >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(x => x.Timestamp <= request.To.Value);

        var totalRecords = await query
            .CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        var records = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return Ok(new LogsResponse
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            Records = records
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiRequestLog>> GetLog(
        int id,
        CancellationToken cancellationToken)
    {
        // NOTE: Comparing FindAsync and FirstOrDefaultAsync
        // 
        // FindAsync checks the EF Core change tracker before hitting the database, so if that entity
        // was already loaded in this context's lifetime it short-circuits without a query at all.
        // FindAsync only accepts a primary key (or composite key) value - it needs that key to look
        // the entity up in the tracker's internal dictionary (keyed by entity type + PK).
        // 
        // FirstOrDefaultAsync does not have this behavior, it always translates into a SQL query and 
        // hits the database every single time. FirstOrDefaultAsync accepts an arbitrary predicate
        // (x => x.SomeColumn == value), so there's no way for EF to short-circuit against the tracker.

        var log = await _db.ApiRequestLogs
            .FindAsync(id, cancellationToken);

        if (log == null)
            return NotFound();

        return Ok(log);
    }
}