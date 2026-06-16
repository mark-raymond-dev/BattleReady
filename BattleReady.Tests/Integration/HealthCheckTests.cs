using System.Net;
using Xunit;

namespace BattleReady.Tests.Integration;

public class HealthCheckTests : IClassFixture<HealthCheckTestFactory>
{
    private readonly HttpClient _client;
    
    public HealthCheckTests(HealthCheckTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_Returns200()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal("Healthy", body);
    }
}