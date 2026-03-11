using AutoMapper;
using FluxoCaixa.Application.DTOs;
using FluxoCaixa.Domain.Interfaces;
using MediatR;

namespace FluxoCaixa.Infrastructure.Commands.Lancamentos;

public class GetLancamentoByIdQueryHandler : IRequestHandler<GetLancamentoByIdQuery, LancamentoDto?>
{
    private readonly ILancamentoRepository _repository;
    private readonly IMapper _mapper;

    public GetLancamentoByIdQueryHandler(ILancamentoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<LancamentoDto?> Handle(GetLancamentoByIdQuery request, CancellationToken cancellationToken)
    {
        var lancamento = await _repository.GetByIdAsync(request.Id);
        return lancamento == null ? null : _mapper.Map<LancamentoDto>(lancamento);
    }
}
