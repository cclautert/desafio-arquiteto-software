using AutoMapper;
using FluxoCaixa.Application.DTOs;
using FluxoCaixa.Domain.Enumerations;
using FluxoCaixa.Domain.Interfaces;
using FluxoCaixa.Domain.Services;
using FluxoCaixa.Infrastructure.Kafka;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluxoCaixa.Infrastructure.Commands.Lancamentos;

public class CreateLancamentoCommandHandler : IRequestHandler<CreateLancamentoCommand, LancamentoDto>
{
    private readonly ILancamentoRepository _repository;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IMapper _mapper;
    private readonly LancamentoDomainService _domainService;
    private readonly ILogger<CreateLancamentoCommandHandler> _logger;

    public CreateLancamentoCommandHandler(
        ILancamentoRepository repository,
        IKafkaProducer kafkaProducer,
        IMapper mapper,
        LancamentoDomainService domainService,
        ILogger<CreateLancamentoCommandHandler> logger)
    {
        _repository = repository;
        _kafkaProducer = kafkaProducer;
        _mapper = mapper;
        _domainService = domainService;
        _logger = logger;
    }

    public async Task<LancamentoDto> Handle(CreateLancamentoCommand request, CancellationToken cancellationToken)
    {
        var (isValid, errorMessage) = _domainService.ValidarLancamento(request.Descricao, request.Valor, request.Tipo);
        if (!isValid)
            throw new ArgumentException(errorMessage);

        var lancamento = _domainService.CriarLancamento(
            request.Descricao,
            request.Valor,
            (TipoLancamento)request.Tipo,
            request.DataLancamento);

        await _repository.AddAsync(lancamento);
        await _repository.SaveChangesAsync();

        var dto = _mapper.Map<LancamentoDto>(lancamento);

        try
        {
            await _kafkaProducer.PublishAsync("lancamento-criado", dto);
            _logger.LogInformation("Lancamento {Id} published to Kafka topic lancamento-criado", lancamento.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish lancamento {Id} to Kafka. Will retry later.", lancamento.Id);
        }

        return dto;
    }
}
