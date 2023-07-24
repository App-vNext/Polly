// Assembly 'Polly.Extensions'

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Polly.Extensions.Telemetry;

namespace Polly;

public static class TelemetryResilienceStrategyBuilderExtensions
{
    public static TBuilder ConfigureTelemetry<TBuilder>(this TBuilder builder, ILoggerFactory loggerFactory) where TBuilder : ResilienceStrategyBuilderBase;
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TelemetryOptions))]
    public static TBuilder ConfigureTelemetry<TBuilder>(this TBuilder builder, TelemetryOptions options) where TBuilder : ResilienceStrategyBuilderBase;
}
