// Assembly 'Polly.Extensions'

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Polly.Telemetry;

namespace Polly;

public static class TelemetryResiliencePipelineBuilderExtensions
{
    public static TBuilder ConfigureTelemetry<TBuilder>(this TBuilder builder, ILoggerFactory loggerFactory) where TBuilder : ResiliencePipelineBuilderBase;
    public static TBuilder ConfigureTelemetry<TBuilder>(this TBuilder builder, TelemetryOptions options) where TBuilder : ResiliencePipelineBuilderBase;
}
