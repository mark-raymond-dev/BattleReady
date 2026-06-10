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

    [HttpGet]
    public async Task<ActionResult<LogsResponse>> GetLogs([FromQuery] LogsRequest request)
    {
        var query = _db.ApiRequestLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Endpoint))
            query = query.Where(x => x.Endpoint.Contains(request.Endpoint));

        if (request.From.HasValue)
            query = query.Where(x => x.Timestamp >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(x => x.Timestamp <= request.To.Value);

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        var records = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

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
    public async Task<ActionResult<ApiRequestLog>> GetLog(int id)
    {
        var log = await _db.ApiRequestLogs.FindAsync(id);

        if (log == null)
            return NotFound();

        return Ok(log);
    }
}