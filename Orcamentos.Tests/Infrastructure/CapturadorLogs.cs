using Microsoft.Extensions.Logging;

namespace Orcamentos.Tests.Infrastructure;

public sealed class CapturadorLogs : ILoggerProvider
{
    private readonly List<(LogLevel Nivel, EventId Evento, string Mensagem)> _registos = [];

    public IReadOnlyList<(LogLevel Nivel, EventId Evento, string Mensagem)> Registos
    {
        get { lock (_registos) return _registos.ToList(); }
    }

    public ILogger CreateLogger(string categoria) => new Logger(this);

    public void Dispose() { }

    private sealed class Logger(CapturadorLogs pai) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel nivel, EventId evento, TState estado,
            Exception? excecao, Func<TState, Exception?, string> formatador)
        {
            lock (pai._registos) pai._registos.Add((nivel, evento, formatador(estado, excecao)));
        }
    }
}
