// Assembly 'Polly.Extensions'

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Polly.Telemetry;

namespace Polly;

public static class TelemetryCompositeStrategyBuilderExtensions
{
    public static TBuilder ConfigureTelemetry<TBuilder>(this TBuilder builder, ILoggerFactory loggerFactory) where TBuilder : CompositeStrategyBuilderBase;
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TelemetryOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TelemetryStrategyOptions))]
    public static TBuilder ConfigureTelemetry<TBuilder>(this TBuilder builder, TelemetryOptions options) where TBuilder : CompositeStrategyBuilderBase;
}
