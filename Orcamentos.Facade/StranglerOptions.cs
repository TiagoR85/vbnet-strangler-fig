namespace Orcamentos.Facade;

public sealed class StranglerOptions
{
    public string Target { get; set; } = "legacy";
    public string LegacyUrl { get; set; } = "http://127.0.0.1:5101";
    public int TimeoutSeconds { get; set; } = 5;

    public void Validar()
    {
        if (Target is not ("legacy" or "modern" or "parity"))
            throw new InvalidOperationException($"Strangler:Target invalido: {Target}");
        if (!Uri.TryCreate(LegacyUrl, UriKind.Absolute, out _))
            throw new InvalidOperationException($"Strangler:LegacyUrl invalido: {LegacyUrl}");
        if (TimeoutSeconds < 1)
            throw new InvalidOperationException("Strangler:TimeoutSeconds deve ser >= 1");
    }
}
