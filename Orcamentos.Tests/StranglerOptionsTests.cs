using Orcamentos.Facade;

namespace Orcamentos.Tests;

public sealed class StranglerOptionsTests
{
    [Theory]
    [InlineData("banana")]
    [InlineData("")]
    [InlineData("PARITY")]
    public void Target_invalido_falha_no_startup(string target)
    {
        var op = new StranglerOptions { Target = target };
        var ex = Assert.Throws<InvalidOperationException>(() => op.Validar());
        Assert.Contains("Strangler:Target invalido", ex.Message);
    }

    [Fact]
    public void Target_em_minusculas_valido()
    {
        var op = new StranglerOptions { Target = "parity" };
        op.Validar();
    }

    [Fact]
    public void LegacyUrl_invalida_falha()
    {
        var op = new StranglerOptions { LegacyUrl = "nao-e-url" };
        Assert.Throws<InvalidOperationException>(() => op.Validar());
    }

    [Fact]
    public void Timeout_minimo_falha()
    {
        var op = new StranglerOptions { TimeoutSeconds = 0 };
        Assert.Throws<InvalidOperationException>(() => op.Validar());
    }

    [Fact]
    public void Defaults_sao_legacy()
    {
        var op = new StranglerOptions();
        op.Validar();
        Assert.Equal("legacy", op.Target);
        Assert.Equal(5, op.TimeoutSeconds);
    }
}
