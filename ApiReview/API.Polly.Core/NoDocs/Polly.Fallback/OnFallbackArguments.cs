// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.Fallback;

public readonly struct OnFallbackArguments<TResult>
{
    public Outcome<TResult> Outcome { get; }
    public ResilienceContext Context { get; }
    public OnFallbackArguments(ResilienceContext context, Outcome<TResult> outcome);
}
