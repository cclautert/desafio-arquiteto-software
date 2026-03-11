namespace FluxoCaixa.Application.DTOs;

public class CreateLancamentoDto
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int Tipo { get; set; }
    public DateTime DataLancamento { get; set; }
}
