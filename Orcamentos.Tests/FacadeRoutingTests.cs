using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Orcamentos.Modern;
using Orcamentos.Tests.Infrastructure;

namespace Orcamentos.Tests;

[Collection("Facade")]
public sealed class FacadeRoutingTests(FacadeFixture facade)
{
    public static IEnumerable<object[]> Goldens =>
        GoldenStore.Carregar().Select(g => new object[] { g });

    private static async Task<(int Status, string Body)> Post(
        WebApplicationFactory<Program> fabrica, Golden golden, string? alvo)
    {
        var cliente = fabrica.CreateClient();
        using var conteudo = new StringContent(golden.RequestJson, Encoding.UTF8, "application/json");
        if (alvo is not null) conteudo.Headers.Add("X-Strangler-Target", alvo);
        using var resposta = await cliente.PostAsync("/orcamentos/calcular", conteudo);
        return ((int)resposta.StatusCode, await resposta.Content.ReadAsStringAsync());
    }

    [Theory]
    [MemberData(nameof(Goldens))]
    public async Task Header_legacy_devolve_verbatim_do_legado(Golden golden)
    {
        var (status, body) = await Post(facade.Fabrica, golden, "legacy");
        Assert.Equal(golden.Status, status);
        Assert.True(JsonCanonico.Igual(golden.Corpo.ToJsonString(), body),
            $"legacy em '{golden.Nome}': esperado={golden.Corpo.ToJsonString()} obtido={body}");
    }

    [Theory]
    [MemberData(nameof(Goldens))]
    public async Task Header_modern_produz_o_mesmo_resultado(Golden golden)
    {
        var (status, body) = await Post(facade.Fabrica, golden, "modern");
        Assert.Equal(golden.Status, status);
        Assert.True(JsonCanonico.Igual(golden.Corpo.ToJsonString(), body),
            $"modern em '{golden.Nome}': esperado={golden.Corpo.ToJsonString()} obtido={body}");
    }

    [Fact]
    public async Task Sem_header_usa_config_legacy()
    {
        var golden = GoldenStore.Carregar().First(g => g.Nome == "pt-happy");
        var (status, body) = await Post(facade.Fabrica, golden, null);
        Assert.Equal(200, status);
        Assert.True(JsonCanonico.Igual(golden.Corpo.ToJsonString(), body));
    }

    [Fact]
    public async Task Header_invalido_devolve_400()
    {
        var cliente = facade.Fabrica.CreateClient();
        using var conteudo = new StringContent("""{"linhas":[],"pais":"PT"}""", Encoding.UTF8, "application/json");
        conteudo.Headers.Add("X-Strangler-Target", "banana");
        using var resposta = await cliente.PostAsync("/orcamentos/calcular", conteudo);
        Assert.Equal(400, (int)resposta.StatusCode);
        Assert.True(JsonCanonico.Igual(
            """{"erro":"header X-Strangler-Target invalido"}""",
            await resposta.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task Health_200_com_legado_vivo()
    {
        var cliente = facade.Fabrica.CreateClient();
        using var resposta = await cliente.GetAsync("/health");
        Assert.Equal(200, (int)resposta.StatusCode);
        Assert.True(JsonCanonico.Igual(
            """{"facade":"ok","legado":"ok","targetConfigurado":"legacy"}""",
            await resposta.Content.ReadAsStringAsync()));
    }
}
