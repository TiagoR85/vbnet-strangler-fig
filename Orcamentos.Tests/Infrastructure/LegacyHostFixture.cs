using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Orcamentos.Tests.Infrastructure;

public sealed class LegacyHostFixture : IDisposable
{
    private readonly Process _processo;

    public int Porta { get; }
    public string BaseUrl => $"http://127.0.0.1:{Porta}";

    public LegacyHostFixture()
    {
        Porta = PortaLivre();
        var exe = LocalizarExe();
        if (!File.Exists(exe))
            throw new InvalidOperationException(
                $"Legado nao encontrado em {exe}. Corre 'dotnet build' na raiz da solucao primeiro.");

        _processo = Process.Start(new ProcessStartInfo(exe, Porta.ToString())
        {
            WorkingDirectory = Path.GetDirectoryName(exe)!,
            UseShellExecute = false
        })!;

        EsperarHealth(TimeSpan.FromSeconds(30));
    }

    private static int PortaLivre()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var porta = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return porta;
    }

    private static string LocalizarExe()
    {
        var configuracao = new DirectoryInfo(AppContext.BaseDirectory).Parent!.Name; // Debug | Release
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
            "LegadoOrcamentos.Vb", "bin", configuracao, "net48", "LegadoOrcamentos.Vb.exe"));
    }

    private void EsperarHealth(TimeSpan limite)
    {
        var fim = DateTime.UtcNow + limite;
        while (DateTime.UtcNow < fim)
        {
            try
            {
                var (status, _) = HttpTeste.Get($"{BaseUrl}/health").GetAwaiter().GetResult();
                if (status == 200) return;
            }
            catch { /* processo ainda a arrancar */ }
            Thread.Sleep(150);
        }
        throw new InvalidOperationException("Legado nao ficou saudavel dentro de 30s.");
    }

    public void Dispose()
    {
        try
        {
            if (!_processo.HasExited) _processo.Kill(entireProcessTree: true);
            _processo.Dispose();
        }
        catch { /* processo ja terminou */ }
    }
}
