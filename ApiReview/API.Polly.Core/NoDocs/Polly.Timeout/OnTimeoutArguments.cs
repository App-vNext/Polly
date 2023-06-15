// Assembly 'Polly.Core'

using System;

namespace Polly.Timeout;

public readonly record struct OnTimeoutArguments(ResilienceContext Context, Exception Exception, TimeSpan Timeout);
