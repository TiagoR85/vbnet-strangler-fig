using System.Text.Json.Nodes;

namespace Orcamentos.Modern;

public interface IOrcamentoCalculadora
{
    JsonObject CalcularJson(string json);
}
