// Assembly 'Polly.Core'

namespace Polly.Retry;

public enum RetryBackoffType
{
    Constant = 0,
    Linear = 1,
    Exponential = 2,
    ExponentialWithJitter = 3
}
