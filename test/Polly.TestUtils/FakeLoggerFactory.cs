using Microsoft.Extensions.Logging;

namespace Polly.TestUtils;

public sealed class FakeLoggerFactory : ILoggerFactory
{
    public FakeLogger FakeLogger { get; } = new FakeLogger();

    public void AddProvider(ILoggerProvider provider)
    {
    }

    public ILogger CreateLogger(string categoryName) => FakeLogger;

    public void Dispose()
    {
    }
}
