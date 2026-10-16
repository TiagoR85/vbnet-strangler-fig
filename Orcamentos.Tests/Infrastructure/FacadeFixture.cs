using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Orcamentos.Tests.Infrastructure;

public sealed class FacadeFixture : IDisposable
{
    public LegacyHostFixture Legado { get; }
    public WebApplicationFactory<Program> Fabrica { get; }

    public FacadeFixture()
    {
        Legado = new LegacyHostFixture();
        Fabrica = Criar(Legado.BaseUrl, "legacy");
    }

    public static WebApplicationFactory<Program> Criar(string legacyUrl, string target) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("Strangler:LegacyUrl", legacyUrl);
            b.UseSetting("Strangler:Target", target);
            b.ConfigureAppConfiguration((_, cfg) => cfg.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Strangler:LegacyUrl"] = legacyUrl,
                    ["Strangler:Target"] = target
                }));
        });

    public void Dispose()
    {
        Fabrica.Dispose();
        Legado.Dispose();
    }
}
