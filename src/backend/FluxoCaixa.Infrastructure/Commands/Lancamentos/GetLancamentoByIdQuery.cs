using FluxoCaixa.Application.DTOs;
using MediatR;

namespace FluxoCaixa.Infrastructure.Commands.Lancamentos;

public record GetLancamentoByIdQuery(Guid Id) : IRequest<LancamentoDto?>;
