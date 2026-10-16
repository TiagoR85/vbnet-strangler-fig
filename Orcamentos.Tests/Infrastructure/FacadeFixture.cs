using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Orcamentos.Tests.Infrastructure;

public sealed class FacadeFixture : IDisposable
{
    public LegacyHostFixture Legado { get; }
    public CapturadorLogs Capturador { get; } = new();
    public WebApplicationFactory<Program> Fabrica { get; }

    public FacadeFixture()
    {
        Legado = new LegacyHostFixture();
        Fabrica = Criar(Legado.BaseUrl, "legacy", Capturador);
    }

    public static WebApplicationFactory<Program> Criar(
        string legacyUrl,
        string target,
        CapturadorLogs? capturador = null,
        Action<IServiceCollection>? extra = null) =>
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
            b.ConfigureServices(s =>
            {
                if (capturador is not null) s.AddSingleton<ILoggerProvider>(capturador);
                extra?.Invoke(s);
            });
        });

    public void Dispose()
    {
        Fabrica.Dispose();
        Legado.Dispose();
    }
}
