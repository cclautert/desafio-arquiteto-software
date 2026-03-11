using FluxoCaixa.Application.DTOs;
using MediatR;

namespace FluxoCaixa.Infrastructure.Commands.Lancamentos;

public record GetAllLancamentosQuery() : IRequest<IEnumerable<LancamentoDto>>;
