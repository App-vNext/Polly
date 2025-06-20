using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Polly.Retry;
using Polly.Timeout;

namespace Polly.PolicyProvider;

/// <summary>
/// Default implementation of resilience policy provider with automatic configuration reloading.
/// </summary>
public sealed class DefaultPolicyProvider : IPolicyProvider, IDisposable
{
    private readonly IOptionsMonitor<PolicyProviderOptions> _optionsMonitor;
    private readonly ConcurrentDictionary<PolicyType, ResiliencePipeline> _policies = new();
    private readonly IDisposable? _optionsChangeToken;
    private volatile bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultPolicyProvider"/> class.
    /// </summary>
    /// <param name="optionsMonitor">The options monitor for policy configuration.</param>
    public DefaultPolicyProvider(IOptionsMonitor<PolicyProviderOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));

        // Subscribe to options changes to regenerate policies
        _optionsChangeToken = _optionsMonitor.OnChange(_ => _policies.Clear());
    }

    /// <inheritdoc/>
    public ResiliencePipeline GetPolicy(PolicyType policyType)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _policies.GetOrAdd(policyType, CreatePolicy);
    }

    /// <summary>
    /// Disposes the policy provider and releases associated resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _optionsChangeToken?.Dispose();
            _policies.Clear();
            _disposed = true;
        }
    }

    private static ResiliencePipeline CreateHttpClientPolicy(PolicyProviderOptions options)
    {
        return new ResiliencePipelineBuilder()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = options.HttpClientTimeout
            })
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = options.HttpClientRetryAttempts,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(500), // Base delay for jitter calculation
                ShouldHandle = new PredicateBuilder()
                    .Handle<TimeoutRejectedException>()
                    .Handle<TaskCanceledException>()
                    .Handle<OperationCanceledException>()
            })
            .Build();
    }

    private ResiliencePipeline CreatePolicy(PolicyType policyType)
    {
        var options = _optionsMonitor.CurrentValue;

        return policyType switch
        {
            PolicyType.HttpClient => CreateHttpClientPolicy(options),
            _ => throw new ArgumentException($"Unsupported policy type: {policyType}", nameof(policyType))
        };
    }
}