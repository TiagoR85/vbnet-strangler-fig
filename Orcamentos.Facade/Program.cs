using System.Text;
using System.Text.Json;
using Orcamentos.Facade;
using Orcamentos.Modern;

var builder = WebApplication.CreateBuilder(args);

var opcoes = builder.Configuration.GetSection("Strangler").Get<StranglerOptions>() ?? new StranglerOptions();
opcoes.Validar();
builder.Services.AddSingleton(opcoes);
builder.Services.AddHttpClient("legado", cliente =>
{
    cliente.BaseAddress = new Uri(opcoes.LegacyUrl);
    cliente.Timeout = TimeSpan.FromSeconds(opcoes.TimeoutSeconds);
});

var app = builder.Build();

app.MapGet("/health", async (StranglerOptions op, IHttpClientFactory fabrica) =>
{
    string legado;
    try
    {
        using var resposta = await fabrica.CreateClient("legado").GetAsync("/health");
        legado = resposta.IsSuccessStatusCode ? "ok" : "down";
    }
    catch
    {
        legado = "down";
    }
    var corpo = new { facade = "ok", legado, targetConfigurado = op.Target };
    return legado == "ok" ? Results.Ok(corpo) : Results.Json(corpo, statusCode: 503);
});

app.MapPost("/orcamentos/calcular", async (HttpRequest pedido, StranglerOptions op,
    IHttpClientFactory fabrica, ILoggerFactory fabricaLogs) =>
{
    string? header = pedido.Headers["X-Strangler-Target"];
    string alvo;
    if (string.IsNullOrEmpty(header))
    {
        alvo = op.Target;
    }
    else
    {
        alvo = header.Trim().ToLowerInvariant();
        if (alvo is not ("legacy" or "modern" or "parity"))
            return Results.Json(new { erro = "header X-Strangler-Target invalido" }, statusCode: 400);
    }

    string corpo;
    using (var leitor = new StreamReader(pedido.Body, Encoding.UTF8))
        corpo = await leitor.ReadToEndAsync();

    if (alvo == "modern")
    {
        var (statusMod, textoMod) = Moderno(corpo);
        return Results.Text(textoMod, "application/json", statusCode: statusMod);
    }

    (int status, string texto) legado;
    try
    {
        using var conteudo = new StringContent(corpo, Encoding.UTF8, "application/json");
        using var resposta = await fabrica.CreateClient("legado").PostAsync("/orcamentos/calcular", conteudo);
        legado = ((int)resposta.StatusCode, await resposta.Content.ReadAsStringAsync());
    }
    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
    {
        return Results.Json(new { erro = "legado indisponivel" }, statusCode: 502);
    }

    if (alvo == "legacy")
        return Results.Text(legado.texto, "application/json", statusCode: legado.status);

    var (statusMod2, textoMod2) = Moderno(corpo);
    var divergencias = statusMod2 == legado.status
        ? JsonCanonico.Divergencias(legado.texto, textoMod2)
        : new List<string> { "status" };
    if (divergencias.Count > 0)
    {
        fabricaLogs.CreateLogger("Parity").LogWarning(
            new EventId(4242, "ParityDiff"),
            "Divergencia legacy vs modern request={Request} campos={Campos}",
            pedido.HttpContext.TraceIdentifier, string.Join(",", divergencias));
    }
    return Results.Text(legado.texto, "application/json", statusCode: legado.status);
});

app.Run();

static (int Status, string Texto) Moderno(string corpo)
{
    try
    {
        return (200, OrcamentoServico.CalcularJson(corpo).ToJsonString());
    }
    catch (CalculoException ex)
    {
        return (400, JsonSerializer.Serialize(new { erro = ex.Message }));
    }
}

public partial class Program { }
