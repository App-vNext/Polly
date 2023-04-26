namespace Polly.Hedging.Utils;

internal readonly record struct ContextSnapshot(ResilienceContext Context, ResilienceProperties OriginalProperties, CancellationToken OriginalCancellationToken);
