using System.Text.Json.Nodes;

namespace Orcamentos.Tests.Infrastructure;

public static class GoldenStore
{
    public static string DiretorioFonte =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "characterization"));

    public static IReadOnlyList<Golden> Carregar(string? diretorio = null)
    {
        var dir = diretorio ?? DiretorioFonte;
        if (!Directory.Exists(dir))
            throw new DirectoryNotFoundException($"Diretorio de goldens nao encontrado: {dir}");
        return Directory.GetFiles(dir, "*.golden.json")
            .OrderBy(f => f, StringComparer.Ordinal)
            .Select(Ler)
            .ToList();
    }

    public static Golden Ler(string caminho)
    {
        var no = JsonNode.Parse(File.ReadAllText(caminho)) as JsonObject
            ?? throw new InvalidOperationException($"Golden invalido: {caminho}");
        var esperado = no["esperado"] as JsonObject
            ?? throw new InvalidOperationException($"esperado ausente: {caminho}");
        return new Golden(
            no["nome"]!.GetValue<string>(),
            no["request"]!.ToJsonString(),
            esperado["status"]!.GetValue<int>(),
            esperado["body"]!.DeepClone());
    }
}
