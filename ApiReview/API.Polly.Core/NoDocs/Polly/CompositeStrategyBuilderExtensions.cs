// Assembly 'Polly.Core'

using System;
using System.Diagnostics.CodeAnalysis;

namespace Polly;

public static class CompositeStrategyBuilderExtensions
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "The EmptyOptions have nothing to validate.")]
    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, ResilienceStrategy strategy) where TBuilder : CompositeStrategyBuilderBase;
    public static CompositeStrategyBuilder<TResult> AddStrategy<TResult>(this CompositeStrategyBuilder<TResult> builder, ResilienceStrategy<TResult> strategy);
    [RequiresUnreferencedCode("This call validates the options using the data annotations attributes.\r\nMake sure that the options are included by adding the '[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(OptionsType))]' attribute to the calling method.")]
    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, Func<StrategyBuilderContext, ResilienceStrategy> factory, ResilienceStrategyOptions options) where TBuilder : CompositeStrategyBuilderBase;
    [RequiresUnreferencedCode("This call validates the options using the data annotations attributes.\r\nMake sure that the options are included by adding the '[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(OptionsType))]' attribute to the calling method.")]
    public static CompositeStrategyBuilder AddStrategy(this CompositeStrategyBuilder builder, Func<StrategyBuilderContext, ReactiveResilienceStrategy<object>> factory, ResilienceStrategyOptions options);
    [RequiresUnreferencedCode("This call validates the options using the data annotations attributes.\r\nMake sure that the options are included by adding the '[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(OptionsType))]' attribute to the calling method.")]
    public static CompositeStrategyBuilder<TResult> AddStrategy<TResult>(this CompositeStrategyBuilder<TResult> builder, Func<StrategyBuilderContext, ReactiveResilienceStrategy<TResult>> factory, ResilienceStrategyOptions options);
}
