using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Enumerations;

namespace FluxoCaixa.Domain.Services;

public class LancamentoDomainService
{
    public (bool IsValid, string? ErrorMessage) ValidarLancamento(string descricao, decimal valor, int tipo)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            return (false, "Descricao is required.");

        if (valor <= 0)
            return (false, "Valor must be greater than zero.");

        if (!Enum.IsDefined(typeof(TipoLancamento), tipo))
            return (false, "Tipo must be 1 (Credito) or 2 (Debito).");

        return (true, null);
    }

    public Lancamento CriarLancamento(string descricao, decimal valor, TipoLancamento tipo, DateTime dataLancamento)
    {
        return new Lancamento(descricao, valor, tipo, dataLancamento);
    }
}
