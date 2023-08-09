// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;
using Polly.Hedging;

namespace Polly;

public static class HedgingCompositeStrategyBuilderExtensions
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "All options members preserved.")]
    public static CompositeStrategyBuilder<TResult> AddHedging<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(this CompositeStrategyBuilder<TResult> builder, HedgingStrategyOptions<TResult> options);
}
