using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Orcamentos.Tests;

public sealed class SolutionScaffoldTests
{
    [Fact]
    public async Task Health_do_scaffold_responde_ok()
    {
        using var fabrica = new WebApplicationFactory<Program>();
        var cliente = fabrica.CreateClient();
        using var resposta = await cliente.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Contains("\"facade\":\"ok\"", await resposta.Content.ReadAsStringAsync());
    }
}
