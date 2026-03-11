namespace FluxoCaixa.Application.DTOs;

public class LancamentoDto
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int Tipo { get; set; }
    public string TipoDescricao { get; set; } = string.Empty;
    public DateTime DataLancamento { get; set; }
    public DateTime CreatedAt { get; set; }
}
