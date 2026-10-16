using System.Text;

namespace Orcamentos.Tests.Infrastructure;

internal static class HttpTeste
{
    private static readonly HttpClient Cliente = new() { Timeout = TimeSpan.FromSeconds(15) };

    public static async Task<(int Status, string Body)> PostJson(string url, string json, string? alvo = null)
    {
        using var conteudo = new StringContent(json, Encoding.UTF8, "application/json");
        if (alvo is not null)
            conteudo.Headers.Add("X-Strangler-Target", alvo);
        using var resposta = await Cliente.PostAsync(url, conteudo);
        return ((int)resposta.StatusCode, await resposta.Content.ReadAsStringAsync());
    }

    public static async Task<(int Status, string Body)> Get(string url)
    {
        using var resposta = await Cliente.GetAsync(url);
        return ((int)resposta.StatusCode, await resposta.Content.ReadAsStringAsync());
    }
}
