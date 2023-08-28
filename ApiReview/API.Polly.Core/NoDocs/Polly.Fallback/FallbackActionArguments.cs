// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.Fallback;

public readonly struct FallbackActionArguments<TResult>
{
    public Outcome<TResult> Outcome { get; }
    public ResilienceContext Context { get; }
    public FallbackActionArguments(ResilienceContext context, Outcome<TResult> outcome);
}
