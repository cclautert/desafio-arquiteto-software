using AutoMapper;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.Configuration;
using FluxoCaixa.Application.DTOs;
using FluxoCaixa.Application.Mappings;
using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces;
using FluxoCaixa.Infrastructure.Commands.ConsolidadoDiario;
using Microsoft.Extensions.Logging;
using Moq;

namespace FluxoCaixa.Tests.Unit.Commands;

public class GetConsolidadoDiarioQueryHandlerTests
{
    private readonly Mock<IConsolidadoDiarioRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<GetConsolidadoDiarioQueryHandler>> _loggerMock;
    private readonly GetConsolidadoDiarioQueryHandler _handler;

    public GetConsolidadoDiarioQueryHandlerTests()
    {
        _repositoryMock = new Mock<IConsolidadoDiarioRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<GetConsolidadoDiarioQueryHandler>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetSection("Redis:CacheTtlMinutes").Value).Returns("5");

        _handler = new GetConsolidadoDiarioQueryHandler(
            _repositoryMock.Object,
            _cacheServiceMock.Object,
            _mapper,
            _loggerMock.Object,
            configurationMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ShouldReturnCachedResult()
    {
        var date = DateTime.UtcNow.Date;
        var cachedDto = new ConsolidadoDiarioDto
        {
            Id = Guid.NewGuid(),
            Data = date,
            TotalCreditos = 500m,
            TotalDebitos = 200m,
            Saldo = 300m
        };

        _cacheServiceMock.Setup(c => c.GetAsync<ConsolidadoDiarioDto>(It.IsAny<string>()))
            .ReturnsAsync(cachedDto);

        var result = await _handler.Handle(new GetConsolidadoDiarioQuery(date), CancellationToken.None);

        result.Should().NotBeNull();
        result!.TotalCreditos.Should().Be(500m);
        result.TotalDebitos.Should().Be(200m);

        _repositoryMock.Verify(r => r.GetByDataAsync(It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ShouldQueryDbAndCacheResult()
    {
        var date = DateTime.UtcNow.Date;
        var consolidado = new ConsolidadoDiario(date);
        consolidado.AddCredito(500m);
        consolidado.AddDebito(200m);

        _cacheServiceMock.Setup(c => c.GetAsync<ConsolidadoDiarioDto>(It.IsAny<string>()))
            .ReturnsAsync((ConsolidadoDiarioDto?)null);
        _repositoryMock.Setup(r => r.GetByDataAsync(date)).ReturnsAsync(consolidado);

        var result = await _handler.Handle(new GetConsolidadoDiarioQuery(date), CancellationToken.None);

        result.Should().NotBeNull();
        result!.TotalCreditos.Should().Be(500m);
        result.TotalDebitos.Should().Be(200m);

        _cacheServiceMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<ConsolidadoDiarioDto>(),
            It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoDataForDate_ShouldReturnNull()
    {
        var date = DateTime.UtcNow.Date;

        _cacheServiceMock.Setup(c => c.GetAsync<ConsolidadoDiarioDto>(It.IsAny<string>()))
            .ReturnsAsync((ConsolidadoDiarioDto?)null);
        _repositoryMock.Setup(r => r.GetByDataAsync(date)).ReturnsAsync((ConsolidadoDiario?)null);

        var result = await _handler.Handle(new GetConsolidadoDiarioQuery(date), CancellationToken.None);

        result.Should().BeNull();
    }
}
