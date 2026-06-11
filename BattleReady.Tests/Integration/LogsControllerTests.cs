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
    private readonly HttpClient _client;
    private readonly LogsTestFactory _factory;

    public LogsControllerTests(LogsTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetLogs_ReturnsEmptyPage_WhenNothingLogged()
    {
        var response = await _client.GetAsync("/api/Logs");

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
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ApiRequestLogs.AddRange(
                new ApiRequestLog { Endpoint = "POST /api/HitChance/calculate", RequestBody = "{}", ResponseBody = "{}", ResponseStatus = 200 },
                new ApiRequestLog { Endpoint = "POST /api/HitChance/calculate", RequestBody = "{}", ResponseBody = "{}", ResponseStatus = 200 },
                new ApiRequestLog { Endpoint = "POST /api/HitChance/calculate", RequestBody = "{}", ResponseBody = "{}", ResponseStatus = 200 }
            );
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/Logs?page=1&pageSize=2");

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
        var response = await _client.GetAsync("/api/Logs/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}