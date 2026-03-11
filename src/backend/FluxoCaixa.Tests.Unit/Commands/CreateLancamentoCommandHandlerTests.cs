using AutoMapper;
using FluentAssertions;
using Xunit;
using FluxoCaixa.Application.DTOs;
using FluxoCaixa.Application.Mappings;
using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces;
using FluxoCaixa.Domain.Services;
using FluxoCaixa.Infrastructure.Commands.Lancamentos;
using FluxoCaixa.Infrastructure.Kafka;
using Microsoft.Extensions.Logging;
using Moq;

namespace FluxoCaixa.Tests.Unit.Commands;

public class CreateLancamentoCommandHandlerTests
{
    private readonly Mock<ILancamentoRepository> _repositoryMock;
    private readonly Mock<IKafkaProducer> _kafkaProducerMock;
    private readonly IMapper _mapper;
    private readonly LancamentoDomainService _domainService;
    private readonly Mock<ILogger<CreateLancamentoCommandHandler>> _loggerMock;
    private readonly CreateLancamentoCommandHandler _handler;

    public CreateLancamentoCommandHandlerTests()
    {
        _repositoryMock = new Mock<ILancamentoRepository>();
        _kafkaProducerMock = new Mock<IKafkaProducer>();
        _domainService = new LancamentoDomainService();
        _loggerMock = new Mock<ILogger<CreateLancamentoCommandHandler>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new CreateLancamentoCommandHandler(
            _repositoryMock.Object,
            _kafkaProducerMock.Object,
            _mapper,
            _domainService,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateAndPublish()
    {
        var command = new CreateLancamentoCommand("Venda produto", 100.50m, 1, DateTime.UtcNow);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Lancamento>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _kafkaProducerMock.Setup(k => k.PublishAsync(It.IsAny<string>(), It.IsAny<LancamentoDto>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Descricao.Should().Be("Venda produto");
        result.Valor.Should().Be(100.50m);
        result.Tipo.Should().Be(1);
        result.TipoDescricao.Should().Be("Credito");

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Lancamento>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _kafkaProducerMock.Verify(k => k.PublishAsync("lancamento-criado", It.IsAny<LancamentoDto>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ShouldThrowArgumentException()
    {
        var command = new CreateLancamentoCommand("", 100m, 1, DateTime.UtcNow);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenKafkaFails_ShouldStillReturnDto()
    {
        var command = new CreateLancamentoCommand("Venda produto", 100m, 1, DateTime.UtcNow);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Lancamento>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _kafkaProducerMock.Setup(k => k.PublishAsync(It.IsAny<string>(), It.IsAny<LancamentoDto>()))
            .ThrowsAsync(new Exception("Kafka unavailable"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Descricao.Should().Be("Venda produto");
    }
}
