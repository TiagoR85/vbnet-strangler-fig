using System.Text.Json;
using System.Text.Json.Nodes;

namespace Orcamentos.Modern;

public static class JsonCanonico
{
    public static bool Igual(string a, string b)
    {
        if (!Tentar(a, out var na) || !Tentar(b, out var nb))
            return string.Equals(a?.Trim(), b?.Trim(), StringComparison.Ordinal);
        return NoIgual(na, nb);
    }

    public static IReadOnlyList<string> Divergencias(string a, string b)
    {
        var lista = new List<string>();
        if (!Tentar(a, out var na) || !Tentar(b, out var nb))
        {
            if (!Igual(a, b)) lista.Add("<raiz>");
            return lista;
        }
        Percorrer(na, nb, "", lista);
        return lista;
    }

    private static bool Tentar(string? s, out JsonNode? no)
    {
        try { no = JsonNode.Parse(s ?? "null"); return true; }
        catch (JsonException) { no = null; return false; }
    }

    private static void Percorrer(JsonNode? a, JsonNode? b, string caminho, List<string> saida)
    {
        if (a is JsonObject oa && b is JsonObject ob)
        {
            foreach (var chave in oa.Select(p => p.Key).Concat(ob.Select(p => p.Key)).Distinct())
            {
                oa.TryGetPropertyValue(chave, out var av);
                ob.TryGetPropertyValue(chave, out var bv);
                var sub = caminho.Length == 0 ? chave : $"{caminho}.{chave}";
                if (av is null || bv is null)
                {
                    if (!NoIgual(av, bv)) saida.Add(sub);
                    continue;
                }
                Percorrer(av, bv, sub, saida);
            }
            return;
        }
        if (a is JsonArray aa && b is JsonArray ba)
        {
            for (var i = 0; i < Math.Max(aa.Count, ba.Count); i++)
            {
                var av = i < aa.Count ? aa[i] : null;
                var bv = i < ba.Count ? ba[i] : null;
                var sub = $"{caminho}[{i}]";
                if (av is null || bv is null)
                {
                    if (!NoIgual(av, bv)) saida.Add(sub);
                    continue;
                }
                Percorrer(av, bv, sub, saida);
            }
            return;
        }
        if (!NoIgual(a, b)) saida.Add(caminho.Length == 0 ? "<raiz>" : caminho);
    }

    private static bool NoIgual(JsonNode? a, JsonNode? b)
    {
        if (a is null || b is null) return a is null && b is null;
        if (a is JsonObject oa && b is JsonObject ob)
        {
            if (oa.Count != ob.Count) return false;
            foreach (var (chave, valor) in oa)
                if (!ob.TryGetPropertyValue(chave, out var outro) || !NoIgual(valor, outro)) return false;
            return true;
        }
        if (a is JsonArray aa && b is JsonArray ba)
        {
            if (aa.Count != ba.Count) return false;
            for (var i = 0; i < aa.Count; i++) if (!NoIgual(aa[i], ba[i])) return false;
            return true;
        }
        if (a is JsonValue va && b is JsonValue vb)
        {
            if (va.TryGetValue<decimal>(out var da) && vb.TryGetValue<decimal>(out var db)) return da == db;
            if (va.TryGetValue<string>(out var sa) && vb.TryGetValue<string>(out var sb)) return sa == sb;
            if (va.TryGetValue<bool>(out var b1) && vb.TryGetValue<bool>(out var b2)) return b1 == b2;
            return false;
        }
        return false;
    }
}
