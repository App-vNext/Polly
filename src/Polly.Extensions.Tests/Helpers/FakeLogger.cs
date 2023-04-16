using Microsoft.Extensions.Logging;

namespace Polly.Extensions.Tests.Helpers;

#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.

public class FakeLogger : ILogger
{
    public List<string> Messages { get; } = new();

    public List<Exception?> Exceptions { get; } = new();

    public List<EventId> Events { get; } = new();

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull => throw new NotSupportedException();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
         where TState : notnull
    {
        Exceptions.Add(exception);
        Messages.Add(formatter(state, exception));
        Events.Add(eventId);
    }
}
