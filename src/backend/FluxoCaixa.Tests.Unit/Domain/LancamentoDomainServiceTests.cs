using FluentAssertions;
using FluxoCaixa.Domain.Enumerations;
using FluxoCaixa.Domain.Services;
using Xunit;

namespace FluxoCaixa.Tests.Unit.Domain;

public class LancamentoDomainServiceTests
{
    private readonly LancamentoDomainService _service;

    public LancamentoDomainServiceTests()
    {
        _service = new LancamentoDomainService();
    }

    [Fact]
    public void ValidarLancamento_WithValidData_ShouldReturnValid()
    {
        var (isValid, errorMessage) = _service.ValidarLancamento("Venda produto", 100.50m, 1);

        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Fact]
    public void ValidarLancamento_WithEmptyDescricao_ShouldReturnInvalid()
    {
        var (isValid, errorMessage) = _service.ValidarLancamento("", 100m, 1);

        isValid.Should().BeFalse();
        errorMessage.Should().Be("Descricao is required.");
    }

    [Fact]
    public void ValidarLancamento_WithZeroValor_ShouldReturnInvalid()
    {
        var (isValid, errorMessage) = _service.ValidarLancamento("Test", 0m, 1);

        isValid.Should().BeFalse();
        errorMessage.Should().Be("Valor must be greater than zero.");
    }

    [Fact]
    public void ValidarLancamento_WithNegativeValor_ShouldReturnInvalid()
    {
        var (isValid, errorMessage) = _service.ValidarLancamento("Test", -10m, 1);

        isValid.Should().BeFalse();
        errorMessage.Should().Be("Valor must be greater than zero.");
    }

    [Fact]
    public void ValidarLancamento_WithInvalidTipo_ShouldReturnInvalid()
    {
        var (isValid, errorMessage) = _service.ValidarLancamento("Test", 100m, 5);

        isValid.Should().BeFalse();
        errorMessage.Should().Be("Tipo must be 1 (Credito) or 2 (Debito).");
    }

    [Fact]
    public void CriarLancamento_ShouldCreateWithCorrectValues()
    {
        var descricao = "Venda produto";
        var valor = 150.75m;
        var tipo = TipoLancamento.Credito;
        var data = DateTime.UtcNow;

        var lancamento = _service.CriarLancamento(descricao, valor, tipo, data);

        lancamento.Should().NotBeNull();
        lancamento.Id.Should().NotBeEmpty();
        lancamento.Descricao.Should().Be(descricao);
        lancamento.Valor.Should().Be(valor);
        lancamento.Tipo.Should().Be(tipo);
        lancamento.DataLancamento.Should().Be(data);
        lancamento.IsCredito().Should().BeTrue();
        lancamento.IsDebito().Should().BeFalse();
    }

    [Fact]
    public void CriarLancamento_Debito_ShouldSetCorrectTipo()
    {
        var lancamento = _service.CriarLancamento("Pagamento fornecedor", 200m, TipoLancamento.Debito, DateTime.UtcNow);

        lancamento.IsDebito().Should().BeTrue();
        lancamento.IsCredito().Should().BeFalse();
    }
}
