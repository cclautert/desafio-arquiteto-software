namespace FluxoCaixa.Domain.Entities;

public class ConsolidadoDiario
{
    public Guid Id { get; private set; }
    public DateTime Data { get; private set; }
    public decimal TotalCreditos { get; private set; }
    public decimal TotalDebitos { get; private set; }
    public decimal Saldo => TotalCreditos - TotalDebitos;
    public DateTime UpdatedAt { get; private set; }

    protected ConsolidadoDiario() { }

    public ConsolidadoDiario(DateTime data)
    {
        Id = Guid.NewGuid();
        Data = data.Date;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddCredito(decimal valor)
    {
        TotalCreditos += valor;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddDebito(decimal valor)
    {
        TotalDebitos += valor;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecalcularSaldo(decimal totalCreditos, decimal totalDebitos)
    {
        TotalCreditos = totalCreditos;
        TotalDebitos = totalDebitos;
        UpdatedAt = DateTime.UtcNow;
    }
}
