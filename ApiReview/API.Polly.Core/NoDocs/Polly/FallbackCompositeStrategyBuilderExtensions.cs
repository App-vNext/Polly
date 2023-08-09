// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;
using Polly.Fallback;

namespace Polly;

public static class FallbackCompositeStrategyBuilderExtensions
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "All options members preserved.")]
    public static CompositeStrategyBuilder<TResult> AddFallback<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(this CompositeStrategyBuilder<TResult> builder, FallbackStrategyOptions<TResult> options);
}
