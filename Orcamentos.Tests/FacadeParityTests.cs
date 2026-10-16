using System.Text;
using Orcamentos.Modern;
using Orcamentos.Tests.Infrastructure;

namespace Orcamentos.Tests;

[Collection("Facade")]
public sealed class FacadeParityTests(FacadeFixture facade)
{
    public static IEnumerable<object[]> Goldens =>
        GoldenStore.Carregar().Select(g => new object[] { g });

    [Theory]
    [MemberData(nameof(Goldens))]
    public async Task Parity_devolve_legado_sem_divergencias(Golden golden)
    {
        var cliente = facade.Fabrica.CreateClient();
        using var conteudo = new StringContent(golden.RequestJson, Encoding.UTF8, "application/json");
        conteudo.Headers.Add("X-Strangler-Target", "parity");
        using var resposta = await cliente.PostAsync("/orcamentos/calcular", conteudo);
        var body = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(golden.Status, (int)resposta.StatusCode);
        Assert.True(JsonCanonico.Igual(golden.Corpo.ToJsonString(), body),
            $"parity em '{golden.Nome}': esperado={golden.Corpo.ToJsonString()} obtido={body}");
        Assert.DoesNotContain(facade.Capturador.Registos, r => r.Evento.Name == "ParityDiff");
    }

    [Fact]
    public async Task Parity_com_legado_down_devolve_502()
    {
        using var fabrica = FacadeFixture.Criar("http://127.0.0.1:1", "parity");
        var cliente = fabrica.CreateClient();
        using var conteudo = new StringContent(
            """{"linhas":[{"sku":"A-1","quantidade":1,"precoUnitario":10.00}],"pais":"PT"}""",
            Encoding.UTF8, "application/json");
        conteudo.Headers.Add("X-Strangler-Target", "parity");
        using var resposta = await cliente.PostAsync("/orcamentos/calcular", conteudo);
        Assert.Equal(502, (int)resposta.StatusCode);
        Assert.True(JsonCanonico.Igual("""{"erro":"legado indisponivel"}""",
            await resposta.Content.ReadAsStringAsync()));
    }
}
