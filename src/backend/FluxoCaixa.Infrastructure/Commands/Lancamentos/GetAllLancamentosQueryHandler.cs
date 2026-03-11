using AutoMapper;
using FluxoCaixa.Application.DTOs;
using FluxoCaixa.Domain.Interfaces;
using MediatR;

namespace FluxoCaixa.Infrastructure.Commands.Lancamentos;

public class GetAllLancamentosQueryHandler : IRequestHandler<GetAllLancamentosQuery, IEnumerable<LancamentoDto>>
{
    private readonly ILancamentoRepository _repository;
    private readonly IMapper _mapper;

    public GetAllLancamentosQueryHandler(ILancamentoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LancamentoDto>> Handle(GetAllLancamentosQuery request, CancellationToken cancellationToken)
    {
        var lancamentos = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<LancamentoDto>>(lancamentos);
    }
}
