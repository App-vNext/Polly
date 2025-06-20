namespace Polly.PolicyProvider;

/// <summary>
/// Defines the types of policies that can be provided by the policy provider.
/// </summary>
public enum PolicyType
{
    /// <summary>
    /// HTTP client policy with timeout and retry capabilities.
    /// </summary>
    HttpClient
}