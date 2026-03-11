using AutoMapper;
using FluentAssertions;
using Xunit;
using FluxoCaixa.Application.DTOs;
using FluxoCaixa.Application.Mappings;
using FluxoCaixa.Application.Services;
using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Enumerations;
using FluxoCaixa.Domain.Interfaces;
using FluxoCaixa.Domain.Services;
using Moq;

namespace FluxoCaixa.Tests.Unit.Application;

public class LancamentoAppServiceTests
{
    private readonly Mock<ILancamentoRepository> _repositoryMock;
    private readonly IMapper _mapper;
    private readonly LancamentoDomainService _domainService;
    private readonly LancamentoAppService _appService;

    public LancamentoAppServiceTests()
    {
        _repositoryMock = new Mock<ILancamentoRepository>();
        _domainService = new LancamentoDomainService();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _appService = new LancamentoAppService(_repositoryMock.Object, _mapper, _domainService);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldReturnDto()
    {
        var dto = new CreateLancamentoDto
        {
            Descricao = "Venda produto",
            Valor = 100.50m,
            Tipo = 1,
            DataLancamento = DateTime.UtcNow
        };

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Lancamento>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _appService.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Descricao.Should().Be(dto.Descricao);
        result.Valor.Should().Be(dto.Valor);
        result.Tipo.Should().Be(dto.Tipo);
        result.Id.Should().NotBeEmpty();

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Lancamento>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidData_ShouldThrowArgumentException()
    {
        var dto = new CreateLancamentoDto
        {
            Descricao = "",
            Valor = 100m,
            Tipo = 1,
            DataLancamento = DateTime.UtcNow
        };

        var act = () => _appService.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Descricao is required.");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnDto()
    {
        var lancamento = new Lancamento("Test", 100m, TipoLancamento.Credito, DateTime.UtcNow);
        _repositoryMock.Setup(r => r.GetByIdAsync(lancamento.Id)).ReturnsAsync(lancamento);

        var result = await _appService.GetByIdAsync(lancamento.Id);

        result.Should().NotBeNull();
        result!.Descricao.Should().Be("Test");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Lancamento?)null);

        var result = await _appService.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllLancamentos()
    {
        var lancamentos = new List<Lancamento>
        {
            new("Venda 1", 100m, TipoLancamento.Credito, DateTime.UtcNow),
            new("Compra 1", 50m, TipoLancamento.Debito, DateTime.UtcNow)
        };

        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(lancamentos);

        var result = await _appService.GetAllAsync();

        result.Should().HaveCount(2);
    }
}
