using FluxoCaixa.Application.DTOs;
using MediatR;

namespace FluxoCaixa.Infrastructure.Commands.Lancamentos;

public record CreateLancamentoCommand(
    string Descricao,
    decimal Valor,
    int Tipo,
    DateTime DataLancamento
) : IRequest<LancamentoDto>;
