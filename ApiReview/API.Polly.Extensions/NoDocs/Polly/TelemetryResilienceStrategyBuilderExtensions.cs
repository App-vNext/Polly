// Assembly 'Polly.Extensions'

using Microsoft.Extensions.Logging;
using Polly.Extensions.Telemetry;

namespace Polly;

public static class TelemetryResilienceStrategyBuilderExtensions
{
    public static TBuilder EnableTelemetry<TBuilder>(this TBuilder builder, ILoggerFactory loggerFactory) where TBuilder : ResilienceStrategyBuilderBase;
    public static TBuilder EnableTelemetry<TBuilder>(this TBuilder builder, TelemetryResilienceStrategyOptions options) where TBuilder : ResilienceStrategyBuilderBase;
}
