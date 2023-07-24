// Assembly 'Polly.Core'

using System;
using System.Diagnostics.CodeAnalysis;
using Polly.Timeout;

namespace Polly;

public static class TimeoutResilienceStrategyBuilderExtensions
{
    public static TBuilder AddTimeout<TBuilder>(this TBuilder builder, TimeSpan timeout) where TBuilder : ResilienceStrategyBuilderBase;
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TimeoutStrategyOptions))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "All options members preserved.")]
    public static TBuilder AddTimeout<TBuilder>(this TBuilder builder, TimeoutStrategyOptions options) where TBuilder : ResilienceStrategyBuilderBase;
}
