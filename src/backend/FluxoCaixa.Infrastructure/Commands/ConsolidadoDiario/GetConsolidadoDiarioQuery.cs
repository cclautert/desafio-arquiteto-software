using FluxoCaixa.Application.DTOs;
using MediatR;

namespace FluxoCaixa.Infrastructure.Commands.ConsolidadoDiario;

public record GetConsolidadoDiarioQuery(DateTime Data) : IRequest<ConsolidadoDiarioDto?>;
