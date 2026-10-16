using System.Text.Json.Nodes;

namespace Orcamentos.Modern;

public sealed class OrcamentoCalculadora : IOrcamentoCalculadora
{
    public JsonObject CalcularJson(string json) => OrcamentoServico.CalcularJson(json);
}
