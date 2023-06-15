// Assembly 'Polly.Core'

using System;
using System.Threading.Tasks;

namespace Polly.Hedging;

public readonly record struct HedgingActionGeneratorArguments<TResult>(ResilienceContext Context, int Attempt, Func<ResilienceContext, ValueTask<Outcome<TResult>>> Callback);
