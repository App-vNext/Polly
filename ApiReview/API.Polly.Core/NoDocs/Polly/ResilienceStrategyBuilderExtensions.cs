// Assembly 'Polly.Core'

using System;
using System.Diagnostics.CodeAnalysis;

namespace Polly;

public static class ResilienceStrategyBuilderExtensions
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "The EmptyOptions have nothing to validate.")]
    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, ResilienceStrategy strategy) where TBuilder : ResilienceStrategyBuilderBase;
    public static ResilienceStrategyBuilder<TResult> AddStrategy<TResult>(this ResilienceStrategyBuilder<TResult> builder, ResilienceStrategy<TResult> strategy);
    [RequiresUnreferencedCode("This call validates the options using the data annotations attributes.\r\nMake sure that the options are included by adding the '[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(OptionsType))]' attribute to the calling method.")]
    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, Func<ResilienceStrategyBuilderContext, ResilienceStrategy> factory, ResilienceStrategyOptions options) where TBuilder : ResilienceStrategyBuilderBase;
}
