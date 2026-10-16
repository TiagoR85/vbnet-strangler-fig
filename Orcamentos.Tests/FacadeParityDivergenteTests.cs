using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orcamentos.Modern;
using Orcamentos.Tests.Infrastructure;

namespace Orcamentos.Tests;

public sealed class FacadeParityDivergenteTests : IDisposable
{
    private readonly LegacyHostFixture _legado = new();
    private readonly CapturadorLogs _capturador = new();
    private readonly WebApplicationFactory<Program> _fabrica;

    public FacadeParityDivergenteTests()
    {
        _fabrica = FacadeFixture.Criar(_legado.BaseUrl, "legacy", _capturador, s =>
        {
            s.AddSingleton<IOrcamentoCalculadora, CalculadoraDivergente>();
        });
    }

    public void Dispose()
    {
        _fabrica.Dispose();
        _legado.Dispose();
    }

    [Fact]
    public async Task Parity_com_moderno_divergente_devolve_legado_e_loga_ParityDiff()
    {
        var golden = GoldenStore.Carregar().First(g => g.Nome == "pt-happy");
        var cliente = _fabrica.CreateClient();
        using var conteudo = new StringContent(golden.RequestJson, Encoding.UTF8, "application/json");
        conteudo.Headers.Add("X-Strangler-Target", "parity");
        using var resposta = await cliente.PostAsync("/orcamentos/calcular", conteudo);
        var body = await resposta.Content.ReadAsStringAsync();

        // shadow puro: a resposta vem SEMPRE do legado, mesmo com o moderno errado
        Assert.Equal(200, (int)resposta.StatusCode);
        Assert.True(JsonCanonico.Igual(golden.Corpo.ToJsonString(), body));

        // e o divergente fica registado no log ParityDiff, com o campo concreto
        var diff = Assert.Single(_capturador.Registos, r => r.Evento.Name == "ParityDiff");
        Assert.Equal(LogLevel.Warning, diff.Nivel);
        Assert.Contains("total", diff.Mensagem);
    }
}
