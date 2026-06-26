using System.Net;
using System.Text.Json;
using Xunit;

namespace BattleReady.Tests.Integration;

public class ExceptionHandlerTests : IClassFixture<ExceptionHandlerTestFactory>
{
    private readonly HttpClient _client;

    public ExceptionHandlerTests(ExceptionHandlerTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UnhandledException_Returns500WithProblemDetails()
    {
        //----------------
        // ACT
        //----------------

        var response = await _client.GetAsync("/test/throw");

        //----------------
        // ASSERT
        //----------------

        // Status code must be 500
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        // Content-Type must be application/problem+json — confirms UseExceptionHandler()
        // is producing a ProblemDetails response rather than an empty body or HTML error page
        var contentType = response.Content.Headers.ContentType?.MediaType;
        Assert.Equal("application/problem+json", contentType);

        // Parse the body and verify the required ProblemDetails fields
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        // RFC 7807 requires "status" and "title" at minimum
        Assert.True(root.TryGetProperty("status", out var statusProp));
        Assert.Equal(500, statusProp.GetInt32());

        Assert.True(root.TryGetProperty("title", out var titleProp));
        Assert.False(string.IsNullOrWhiteSpace(titleProp.GetString()));
    }

    [Fact]
    public async Task NotFoundException_Returns404WithProblemDetails()
    {
        var response = await _client.GetAsync("/test/not-found");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(body).RootElement;

        Assert.Equal(404, root.GetProperty("status").GetInt32());
        Assert.Equal("Resource not found.", root.GetProperty("title").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("detail").GetString()));
    }

    [Fact]
    public async Task ValidationException_Returns422WithProblemDetails()
    {
        var response = await _client.GetAsync("/test/validation");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(body).RootElement;

        Assert.Equal(422, root.GetProperty("status").GetInt32());
        Assert.Equal("Unprocessable entity.", root.GetProperty("title").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("detail").GetString()));
    }

    [Fact]
    public async Task DomainException_Returns400WithProblemDetails()
    {
        var response = await _client.GetAsync("/test/domain");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(body).RootElement;

        Assert.Equal(400, root.GetProperty("status").GetInt32());
        Assert.Equal("Bad request.", root.GetProperty("title").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("detail").GetString()));
    }
}