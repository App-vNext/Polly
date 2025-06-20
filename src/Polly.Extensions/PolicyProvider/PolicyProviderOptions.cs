using System.ComponentModel.DataAnnotations;

namespace Polly.PolicyProvider;

/// <summary>
/// Configuration options for the policy provider.
/// </summary>
public class PolicyProviderOptions
{
    /// <summary>
    /// Gets or sets the number of retry attempts for HTTP client policies.
    /// </summary>
    /// <value>
    /// The default value is 3 attempts.
    /// </value>
    [Range(1, 10)]
    public int HttpClientRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the timeout duration for HTTP client policies.
    /// </summary>
    /// <value>
    /// The default value is 15 seconds.
    /// </value>
    [Range(typeof(TimeSpan), "00:00:01", "00:05:00")]
    public TimeSpan HttpClientTimeout { get; set; } = TimeSpan.FromSeconds(15);
}