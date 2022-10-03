#nullable enable
using System;

namespace Polly.Retry.Options;

public class RetryPolicyOptionsBase
{
    private RetryCountValue _permittedRetryCount = RetryCountValue.Infinity;
    public RetryCountValue PermittedRetryCount
    {
        get => _permittedRetryCount;
        set => _permittedRetryCount = value ?? throw new ArgumentNullException(nameof(value));
    }
}