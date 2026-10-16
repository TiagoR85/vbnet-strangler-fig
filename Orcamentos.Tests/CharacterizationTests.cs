using System.Text.Json;
using System.Text.Json.Nodes;
using Orcamentos.Modern;
using Orcamentos.Tests.Infrastructure;

namespace Orcamentos.Tests;

[Collection("Legado")]
public sealed class CharacterizationTests(LegacyHostFixture legado)
{
    public static IEnumerable<object[]> Goldens =>
        GoldenStore.Carregar().Select(g => new object[] { g });

    [Theory]
    [MemberData(nameof(Goldens))]
    public async Task Legado_congela_comportamento(Golden golden)
    {
        var (status, body) = await HttpTeste.PostJson($"{legado.BaseUrl}/orcamentos/calcular", golden.RequestJson);

        if (Environment.GetEnvironmentVariable("CAPTURE_GOLDENS") == "1")
        {
            var ficheiro = Path.Combine(GoldenStore.DiretorioFonte, $"{golden.Nome}.golden.json");
            var capturado = new JsonObject
            {
                ["nome"] = golden.Nome,
                ["request"] = JsonNode.Parse(golden.RequestJson)!.DeepClone(),
                ["esperado"] = new JsonObject
                {
                    ["status"] = status,
                    ["body"] = JsonNode.Parse(body)!.DeepClone()
                }
            };
            File.WriteAllText(ficheiro,
                capturado.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            return;
        }

        Assert.Equal(golden.Status, status);
        Assert.True(JsonCanonico.Igual(golden.Corpo.ToJsonString(), body),
            $"Divergencia em '{golden.Nome}':\nesperado={golden.Corpo.ToJsonString()}\nobtido={body}");
    }

    [Fact]
    public async Task Health_do_legado_responde_ok()
    {
        var (status, body) = await HttpTeste.Get($"{legado.BaseUrl}/health");
        Assert.Equal(200, status);
        Assert.True(JsonCanonico.Igual("""{"status":"ok"}""", body));
    }
}
