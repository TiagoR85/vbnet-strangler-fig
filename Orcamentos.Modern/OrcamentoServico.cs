using System.Text.Json;
using System.Text.Json.Nodes;

namespace Orcamentos.Modern;

public sealed class CalculoException(string mensagem) : Exception(mensagem);

public sealed record LinhaPedido(string? Sku, int Quantidade, decimal PrecoUnitario);

public sealed record PedidoOrcamento(List<LinhaPedido>? Linhas, string? Pais);

public static class OrcamentoServico
{
    private static readonly JsonSerializerOptions Opcoes = new() { PropertyNameCaseInsensitive = true };

    public static JsonObject CalcularJson(string json)
    {
        try
        {
            var pedido = JsonSerializer.Deserialize<PedidoOrcamento>(json, Opcoes);
            if (pedido is null) throw new CalculoException("json invalido");
            return Calcular(pedido);
        }
        catch (JsonException)
        {
            throw new CalculoException("json invalido");
        }
    }

    public static JsonObject Calcular(PedidoOrcamento? pedido)
    {
        if (pedido?.Linhas is null || pedido.Linhas.Count == 0)
            throw new CalculoException("pelo menos uma linha");

        decimal subtotal = 0;
        foreach (var linha in pedido.Linhas)
        {
            if (linha is null || string.IsNullOrWhiteSpace(linha.Sku))
                throw new CalculoException("linha invalida");
            if (linha.Quantidade < 1)
                throw new CalculoException("quantidade deve ser >= 1");
            if (linha.PrecoUnitario < 0)
                throw new CalculoException("precoUnitario deve ser >= 0");
            subtotal += linha.Quantidade * linha.PrecoUnitario;
        }

        decimal pct = subtotal switch
        {
            < 100m => 0m,
            < 500m => 2m,
            < 1000m => 5m,
            _ => 8m
        };
        var desconto = Math.Round(subtotal * pct / 100m, 2, MidpointRounding.ToEven);
        var baseValor = subtotal - desconto;

        var (ivaPct, portesCheio) = pedido.Pais switch
        {
            "PT" => (23m, 4.9m),
            "ES" => (21m, 9.9m),
            "FR" => (20m, 14.9m),
            "OUT" => (0m, 19.9m),
            _ => throw new CalculoException("pais desconhecido")
        };

        var portes = subtotal >= 500m ? 0m : portesCheio;
        var iva = Math.Round(baseValor * ivaPct / 100m, 2, MidpointRounding.ToEven);
        var total = baseValor + iva + portes;

        return new JsonObject
        {
            ["subtotal"] = subtotal,
            ["descontoPercent"] = pct,
            ["descontoEuros"] = desconto,
            ["base"] = baseValor,
            ["ivaPercent"] = ivaPct,
            ["ivaEuros"] = iva,
            ["portes"] = portes,
            ["total"] = total,
            ["moeda"] = "EUR"
        };
    }
}
