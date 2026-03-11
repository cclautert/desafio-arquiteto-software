using System.Net;
using System.Net.Http.Headers;
using Xunit;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using FluxoCaixa.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using LancamentosProgram = FluxoCaixa.API.Lancamentos.Program;

namespace FluxoCaixa.Tests.Integration;

public class LancamentosApiTests : IClassFixture<WebApplicationFactory<LancamentosProgram>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<LancamentosProgram> _factory;

    public LancamentosApiTests(WebApplicationFactory<LancamentosProgram> factory)
    {
        _factory = factory;
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
    public async Task PostLancamento_WithValidData_ShouldReturn201()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var lancamento = new CreateLancamentoDto
        {
            Descricao = "Venda Integration Test",
            Valor = 250.00m,
            Tipo = 1,
            DataLancamento = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/v1/lancamentos", lancamento);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<LancamentoDto>();
        result.Should().NotBeNull();
        result!.Descricao.Should().Be("Venda Integration Test");
        result.Valor.Should().Be(250.00m);
    }

    [Fact]
    public async Task PostLancamento_WithoutAuth_ShouldReturn401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var lancamento = new CreateLancamentoDto
        {
            Descricao = "Test",
            Valor = 100m,
            Tipo = 1,
            DataLancamento = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/v1/lancamentos", lancamento);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLancamentos_WithAuth_ShouldReturn200()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/lancamentos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturn200()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GenerateToken_WithValidCredentials_ShouldReturnToken()
    {
        var loginRequest = new { Username = "testuser", Password = "testpass" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/token", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);
        tokenResponse.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }
}
