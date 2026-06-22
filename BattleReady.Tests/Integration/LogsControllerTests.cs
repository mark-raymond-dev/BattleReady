using System.Net;
using System.Net.Http.Json;
using BattleReady.Api.Models.Requests;
using BattleReady.Api.Models.Responses;
using BattleReady.Data;
using BattleReady.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BattleReady.Tests.Integration;

public class LogsControllerTests : IClassFixture<LogsTestFactory>
{
    private const string Version = "v1";
    private const string LogsUrl = $"/api/{Version}/Logs";

    private readonly HttpClient _client;
    private readonly LogsTestFactory _factory;

    public LogsControllerTests(LogsTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Clear all log entries before each test so tests don't bleed into each other.
        // All tests in this class either seed their own data or assert on an empty database,
        // so a clean slate at the start of each test is the correct baseline.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.ApiRequestLogs.RemoveRange(db.ApiRequestLogs);
        db.SaveChanges();
    }

    [Fact]
    public async Task GetLogs_ReturnsEmptyPage_WhenNothingLogged()
    {
        // base route, no query string
        var response = await _client.GetAsync(LogsUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LogsResponse>();
        Assert.NotNull(body);
        Assert.Equal(0, body.TotalRecords);
        Assert.Equal(0, body.TotalPages);
        Assert.Empty(body.Records);
    }

    [Fact]
    public async Task GetLogs_ReturnsPaginatedResults_AfterRequestsAreLogged()
    {
        // Seed 3 log entries directly via the DB
        var hitChanceLoggedEndpoint = "POST /api/v1/HitChance/calculate";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ApiRequestLogs.AddRange(
                new ApiRequestLog { Endpoint = hitChanceLoggedEndpoint, RequestBody = "{}", ResponseBody = "{}", ResponseStatus = 200 },
                new ApiRequestLog { Endpoint = hitChanceLoggedEndpoint, RequestBody = "{}", ResponseBody = "{}", ResponseStatus = 200 },
                new ApiRequestLog { Endpoint = hitChanceLoggedEndpoint, RequestBody = "{}", ResponseBody = "{}", ResponseStatus = 200 }
            );
            await db.SaveChangesAsync();
        }

        // base route + query string — query string stays inline since it's test-specific data, not a fixed route
        var response = await _client.GetAsync($"{LogsUrl}?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LogsResponse>();
        Assert.NotNull(body);
        Assert.Equal(3, body.TotalRecords);
        Assert.Equal(2, body.TotalPages);   // ceil(3/2) = 2
        Assert.Equal(2, body.Records.Count);
    }

    [Fact]
    public async Task GetLog_ReturnsNotFound_WhenIdDoesNotExist()
    {
        // base route + id — id is also test-specific data (99999 is a deliberately-nonexistent id for this particular test)
        var response = await _client.GetAsync($"{LogsUrl}/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLogs_FiltersByEndpointPartialMatch()
    {
        //----------------
        // ARRANGE
        //----------------

        // Seed 3 log entries directly via the DB ... 2 for Calculator, 1 for HitChance
        string targetEndpoint  = "POST /api/v1/Calculator/calculate";
        string otherEndpoint   = "POST /api/v1/HitChance/calculate";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ApiRequestLogs.AddRange(
                new ApiRequestLog { Endpoint = targetEndpoint, RequestBody = "{}", ResponseBody = "{}", ResponseStatus = 200 },
                new ApiRequestLog { Endpoint = targetEndpoint, RequestBody = "{}", ResponseBody = "{}", ResponseStatus = 200 },
                new ApiRequestLog { Endpoint = otherEndpoint,  RequestBody = "{}", ResponseBody = "{}", ResponseStatus = 200 }
            );
            await db.SaveChangesAsync();
        }

        //----------------
        // ACT
        //----------------

        // "Calculator" is a partial match — should match targetEndpoint but not otherEndpoint
        var response = await _client.GetAsync($"{LogsUrl}?endpoint=Calculator");

        //----------------
        // ASSERT
        //----------------

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LogsResponse>();
        Assert.NotNull(body);
        Assert.Equal(2, body.TotalRecords);
        Assert.All(body.Records, r => Assert.Contains("Calculator", r.Endpoint));
    }

    [Fact]
    public async Task GetLogs_FiltersByFromDate()
    {
        //----------------
        // ARRANGE
        //----------------

        // Seed 2 log entries directly via the DB ... 1 with older timestamp, 1 with more recent timestamp
        string targetEndpoint  = "POST /api/v1/Calculator/calculate";
        var oldTimestamp    = DateTime.UtcNow.AddDays(-10);
        var recentTimestamp = DateTime.UtcNow.AddDays(-1);
        var filterFrom      = DateTime.UtcNow.AddDays(-5);  // should exclude the old record
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ApiRequestLogs.AddRange(
                new ApiRequestLog { Endpoint = targetEndpoint, Timestamp = oldTimestamp,    RequestBody = "{}", ResponseBody = "{}", ResponseStatus = 200 },
                new ApiRequestLog { Endpoint = targetEndpoint, Timestamp = recentTimestamp, RequestBody = "{}", ResponseBody = "{}", ResponseStatus = 200 }
            );
            await db.SaveChangesAsync();
        }

        //----------------
        // ACT
        //----------------

        var response = await _client.GetAsync($"{LogsUrl}?from={filterFrom:O}");

        //----------------
        // ASSERT
        //----------------

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LogsResponse>();
        Assert.NotNull(body);
        Assert.Equal(1, body.TotalRecords);   // only the recent one
    }

    [Fact]
    public async Task GetLogs_FiltersByToDate()
    {
        //----------------
        // ARRANGE
        //----------------

        // Seed 2 log entries directly via the DB ... 1 with older timestamp, 1 with more recent timestamp
        string targetEndpoint  = "POST /api/v1/Calculator/calculate";
        var oldTimestamp    = DateTime.UtcNow.AddDays(-10);
        var recentTimestamp = DateTime.UtcNow.AddDays(-1);
        var filterTo        = DateTime.UtcNow.AddDays(-5);  // should exclude the recent record
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ApiRequestLogs.AddRange(
                new ApiRequestLog { Endpoint = targetEndpoint, Timestamp = oldTimestamp,    RequestBody = "{}", ResponseBody = "{}", ResponseStatus = 200 },
                new ApiRequestLog { Endpoint = targetEndpoint, Timestamp = recentTimestamp, RequestBody = "{}", ResponseBody = "{}", ResponseStatus = 200 }
            );
            await db.SaveChangesAsync();
        }

        //----------------
        // ACT
        //----------------

        var response = await _client.GetAsync($"{LogsUrl}?to={filterTo:O}");

        //----------------
        // ASSERT
        //----------------

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LogsResponse>();
        Assert.NotNull(body);
        Assert.Equal(1, body.TotalRecords);   // only the old one
    }
}