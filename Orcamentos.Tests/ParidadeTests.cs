using Orcamentos.Modern;
using Orcamentos.Tests.Infrastructure;

namespace Orcamentos.Tests;

[Collection("Legado")]
public sealed class ParidadeTests(LegacyHostFixture legado)
{
    public static IEnumerable<object[]> Goldens =>
        GoldenStore.Carregar().Select(g => new object[] { g });

    [Theory]
    [MemberData(nameof(Goldens))]
    public async Task Modern_reproduz_o_legado(Golden golden)
    {
        var (statusLegado, corpoLegado) =
            await HttpTeste.PostJson($"{legado.BaseUrl}/orcamentos/calcular", golden.RequestJson);

        int statusModern;
        string corpoModern;
        try
        {
            corpoModern = OrcamentoServico.CalcularJson(golden.RequestJson).ToJsonString();
            statusModern = 200;
        }
        catch (CalculoException ex)
        {
            statusModern = 400;
            corpoModern = $$"""{"erro":"{{ex.Message}}"}""";
        }

        Assert.Equal(statusLegado, statusModern);
        Assert.True(JsonCanonico.Igual(corpoLegado, corpoModern),
            $"Paridade divergente em '{golden.Nome}':\nlegado={corpoLegado}\nmodern={corpoModern}");
    }

    [Fact]
    public void Modern_rejeita_json_invalido()
    {
        var ex = Assert.Throws<CalculoException>(() => OrcamentoServico.CalcularJson("{{{"));
        Assert.Equal("json invalido", ex.Message);
    }

    [Fact]
    public void Modern_rejeita_body_nulo()
    {
        var ex = Assert.Throws<CalculoException>(() => OrcamentoServico.CalcularJson("null"));
        Assert.Equal("json invalido", ex.Message);
    }
}
