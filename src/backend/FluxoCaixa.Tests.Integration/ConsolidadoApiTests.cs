using System.Net;
using System.Net.Http.Headers;
using Xunit;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using ConsolidadoProgram = FluxoCaixa.API.Consolidado.Program;

namespace FluxoCaixa.Tests.Integration;

public class ConsolidadoApiTests : IClassFixture<WebApplicationFactory<ConsolidadoProgram>>
{
    private readonly HttpClient _client;

    public ConsolidadoApiTests(WebApplicationFactory<ConsolidadoProgram> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetTokenAsync()
    {
        var loginRequest = new { Username = "testuser", Password = "testpass" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/token", loginRequest);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);
        return tokenResponse.GetProperty("token").GetString()!;
    }

    [Fact]
    public async Task GetConsolidadoToday_WithAuth_ShouldReturn404WhenNoData()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/consolidado");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetConsolidado_WithoutAuth_ShouldReturn401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/v1/consolidado");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetConsolidadoByDate_WithAuth_ShouldReturn404WhenNoData()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var response = await _client.GetAsync($"/api/v1/consolidado/{date}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturn200()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
