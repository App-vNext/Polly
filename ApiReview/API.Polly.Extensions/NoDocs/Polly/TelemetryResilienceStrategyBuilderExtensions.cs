// Assembly 'Polly.Extensions'

using Microsoft.Extensions.Logging;
using Polly.Extensions.Telemetry;

namespace Polly;

public static class TelemetryResilienceStrategyBuilderExtensions
{
    public static TBuilder ConfigureTelemetry<TBuilder>(this TBuilder builder, ILoggerFactory loggerFactory) where TBuilder : ResilienceStrategyBuilderBase;
    public static TBuilder ConfigureTelemetry<TBuilder>(this TBuilder builder, TelemetryOptions options) where TBuilder : ResilienceStrategyBuilderBase;
}
