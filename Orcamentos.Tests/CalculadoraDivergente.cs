using System.Text.Json.Nodes;
using Orcamentos.Modern;

namespace Orcamentos.Tests;

public sealed class CalculadoraDivergente : IOrcamentoCalculadora
{
    private readonly OrcamentoCalculadora _real = new();

    public JsonObject CalcularJson(string json)
    {
        var resposta = _real.CalcularJson(json);
        resposta["total"] = resposta["total"]!.GetValue<decimal>() + 0.01m;
        return resposta;
    }
}
