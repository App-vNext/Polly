using Microsoft.Extensions.Logging;

namespace Polly.TestUtils;
#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.

public record class LogRecord(EventId EventId, string Message, Exception? Exception);
