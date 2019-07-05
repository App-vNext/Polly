using System;

namespace Polly.Specs.Helpers.RateLimit
{
    internal class ResultClassWithRetryAfter : ResultClass
    {
        public TimeSpan RetryAfter { get; }

        public ResultClassWithRetryAfter(ResultPrimitive result)
        : base(result)
        {
            RetryAfter = TimeSpan.Zero;
        }

        public ResultClassWithRetryAfter(TimeSpan retryAfter)
        : base(ResultPrimitive.Undefined)
        {
            RetryAfter = retryAfter;
        }
    }
}
