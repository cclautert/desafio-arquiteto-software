using FluxoCaixa.Domain.Enumerations;

namespace FluxoCaixa.Domain.Entities;

public class Lancamento
{
    public Guid Id { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public TipoLancamento Tipo { get; private set; }
    public DateTime DataLancamento { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Lancamento() { }

    public Lancamento(string descricao, decimal valor, TipoLancamento tipo, DateTime dataLancamento)
    {
        Id = Guid.NewGuid();
        Descricao = descricao;
        Valor = valor;
        Tipo = tipo;
        DataLancamento = dataLancamento;
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsCredito() => Tipo == TipoLancamento.Credito;
    public bool IsDebito() => Tipo == TipoLancamento.Debito;
}
