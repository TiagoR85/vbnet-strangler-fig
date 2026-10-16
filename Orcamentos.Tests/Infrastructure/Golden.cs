using System.Text.Json.Nodes;

namespace Orcamentos.Tests.Infrastructure;

public sealed record Golden(string Nome, string RequestJson, int Status, JsonNode Corpo);
