using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Orcamentos.Modern;
using Orcamentos.Tests.Infrastructure;

namespace Orcamentos.Tests;

public sealed class FacadeForaDeServicoTests
{
    private const string UrlMorta = "http://127.0.0.1:1";
    private static readonly Golden Aproveitavel =
        GoldenStore.Carregar().First(g => g.Nome == "pt-happy");

    private static Task<(int Status, string Body)> Post(
        WebApplicationFactory<Program> fabrica, string? alvo = null)
    {
        var cliente = fabrica.CreateClient();
        using var conteudo = new StringContent(Aproveitavel.RequestJson, Encoding.UTF8, "application/json");
        if (alvo is not null) conteudo.Headers.Add("X-Strangler-Target", alvo);
        using var resposta = cliente.PostAsync("/orcamentos/calcular", conteudo).Result;
        return Task.FromResult(((int)resposta.StatusCode, resposta.Content.ReadAsStringAsync().Result));
    }

    [Fact]
    public async Task Legado_em_baixo_devolve_502()
    {
        var (status, body) = await Post(FacadeFixture.Criar(UrlMorta, "legacy"));
        Assert.Equal(502, status);
        Assert.True(JsonCanonico.Igual("""{"erro":"legado indisponivel"}""", body));
    }

    [Fact]
    public async Task Health_503_com_legado_down()
    {
        var fabrica = FacadeFixture.Criar(UrlMorta, "legacy");
        var cliente = fabrica.CreateClient();
        using var resposta = await cliente.GetAsync("/health");
        Assert.Equal(503, (int)resposta.StatusCode);
        Assert.True(JsonCanonico.Igual(
            """{"facade":"ok","legado":"down","targetConfigurado":"legacy"}""",
            await resposta.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task Config_modern_sem_header_nao_toca_no_legado()
    {
        var (status, body) = await Post(FacadeFixture.Criar(UrlMorta, "modern"));
        Assert.Equal(200, status);
        Assert.True(JsonCanonico.Igual(Aproveitavel.Corpo.ToJsonString(), body));
    }

    [Fact]
    public async Task Header_legacy_prevalece_sobre_config_modern()
    {
        var (status, body) = await Post(FacadeFixture.Criar(UrlMorta, "modern"), "legacy");
        Assert.Equal(502, status);
        Assert.True(JsonCanonico.Igual("""{"erro":"legado indisponivel"}""", body));
    }

    [Fact]
    public async Task Header_modern_prevalece_sobre_config_legacy()
    {
        var (status, body) = await Post(FacadeFixture.Criar(UrlMorta, "legacy"), "modern");
        Assert.Equal(200, status);
        Assert.True(JsonCanonico.Igual(Aproveitavel.Corpo.ToJsonString(), body));
    }
}
