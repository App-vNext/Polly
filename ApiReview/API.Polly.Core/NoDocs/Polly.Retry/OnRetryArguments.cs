// Assembly 'Polly.Core'

using System;

namespace Polly.Retry;

public readonly record struct OnRetryArguments(int Attempt, TimeSpan RetryDelay);
