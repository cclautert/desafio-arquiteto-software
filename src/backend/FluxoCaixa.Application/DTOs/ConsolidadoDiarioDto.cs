namespace FluxoCaixa.Application.DTOs;

public class ConsolidadoDiarioDto
{
    public Guid Id { get; set; }
    public DateTime Data { get; set; }
    public decimal TotalCreditos { get; set; }
    public decimal TotalDebitos { get; set; }
    public decimal Saldo { get; set; }
    public DateTime UpdatedAt { get; set; }
}
