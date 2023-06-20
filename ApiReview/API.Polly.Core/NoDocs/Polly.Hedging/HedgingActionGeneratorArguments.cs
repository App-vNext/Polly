// Assembly 'Polly.Core'

using System;
using System.Threading.Tasks;

namespace Polly.Hedging;

public readonly record struct HedgingActionGeneratorArguments<TResult>(ResilienceContext PrimaryContext, ResilienceContext ActionContext, int Attempt, Func<ResilienceContext, ValueTask<Outcome<TResult>>> Callback);
