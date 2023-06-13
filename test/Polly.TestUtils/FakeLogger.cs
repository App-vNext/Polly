using Microsoft.Extensions.Logging;

namespace Polly.TestUtils;

#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.

public class FakeLogger : ILogger
{
    private readonly List<LogRecord> _records = new();

    public IEnumerable<LogRecord> GetRecords() => _records;

    public IEnumerable<LogRecord> GetRecords(EventId eventId) => GetRecords().Where(v => v.EventId.Id == eventId.Id && v.EventId.Name == eventId.Name);

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
         where TState : notnull
    {
        _records.Add(new LogRecord(eventId, formatter(state, exception), exception));
    }
}
