// Assembly 'Polly.Core'

using System;

namespace Polly.Retry;

public readonly record struct RetryDelayArguments(int Attempt, TimeSpan DelayHint);
