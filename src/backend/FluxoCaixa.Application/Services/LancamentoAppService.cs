using AutoMapper;
using FluxoCaixa.Application.DTOs;
using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Enumerations;
using FluxoCaixa.Domain.Interfaces;
using FluxoCaixa.Domain.Services;

namespace FluxoCaixa.Application.Services;

public class LancamentoAppService : ILancamentoAppService
{
    private readonly ILancamentoRepository _repository;
    private readonly IMapper _mapper;
    private readonly LancamentoDomainService _domainService;

    public LancamentoAppService(
        ILancamentoRepository repository,
        IMapper mapper,
        LancamentoDomainService domainService)
    {
        _repository = repository;
        _mapper = mapper;
        _domainService = domainService;
    }

    public async Task<LancamentoDto> CreateAsync(CreateLancamentoDto dto)
    {
        var (isValid, errorMessage) = _domainService.ValidarLancamento(dto.Descricao, dto.Valor, dto.Tipo);
        if (!isValid)
            throw new ArgumentException(errorMessage);

        var lancamento = _domainService.CriarLancamento(
            dto.Descricao,
            dto.Valor,
            (TipoLancamento)dto.Tipo,
            dto.DataLancamento);

        await _repository.AddAsync(lancamento);
        await _repository.SaveChangesAsync();

        return _mapper.Map<LancamentoDto>(lancamento);
    }

    public async Task<LancamentoDto?> GetByIdAsync(Guid id)
    {
        var lancamento = await _repository.GetByIdAsync(id);
        return lancamento == null ? null : _mapper.Map<LancamentoDto>(lancamento);
    }

    public async Task<IEnumerable<LancamentoDto>> GetAllAsync()
    {
        var lancamentos = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<LancamentoDto>>(lancamentos);
    }
}
