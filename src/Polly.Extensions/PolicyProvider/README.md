# Policy Provider

The Policy Provider is a convenient abstraction for managing and providing pre-configured Polly resilience pipelines based on policy type identifiers.

## Features

- **Enum-based policy identification**: Use `PolicyType` enum to request specific policies
- **HTTP client timeout and retry support**: Built-in policy for HTTP client scenarios with pessimistic cancellation
- **Configurable retry attempts and timeouts**: Load settings from configuration
- **Automatic policy regeneration**: Policies are automatically recreated when configuration changes
- **Policy caching and reuse**: Policies are created once and cached for performance
- **Exponential backoff with jitter**: Uses exponential backoff with jitter for retry delays

## Basic Usage

```csharp
// Register the policy provider
services.AddPolicyProvider(options =>
{
    options.HttpClientRetryAttempts = 3;      // Retry 3 times
    options.HttpClientTimeout = TimeSpan.FromSeconds(15); // 15 second timeout
});

// Use the policy provider
var policyProvider = serviceProvider.GetRequiredService<IPolicyProvider>();
var httpPolicy = policyProvider.GetPolicy(PolicyType.HttpClient);

// Execute with policy
var result = await httpPolicy.ExecuteAsync(async cancellationToken =>
{
    // Your HTTP call here
    return await httpClient.GetAsync("https://api.example.com", cancellationToken);
});
```

## Configuration

The `PolicyProviderOptions` class allows you to configure:

- `HttpClientRetryAttempts`: Number of retry attempts (default: 3)
- `HttpClientTimeout`: Total timeout for the operation (default: 15 seconds)

## Policy Details

### HttpClient Policy

The HTTP client policy combines:

1. **Timeout Strategy**: Applies the configured timeout with pessimistic cancellation
2. **Retry Strategy**: Retries on timeout and cancellation exceptions with:
   - Exponential backoff
   - Jitter to avoid thundering herd
   - Configurable retry attempts
   - Base delay of 500ms for jitter calculation

## Automatic Reload

When using `IOptionsMonitor`, the policy provider automatically clears cached policies when configuration changes, causing them to be recreated with new settings on next access.