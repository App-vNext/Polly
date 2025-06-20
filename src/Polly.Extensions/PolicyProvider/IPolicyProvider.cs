namespace Polly.PolicyProvider;

/// <summary>
/// Provides resilience pipelines based on policy type identifiers.
/// </summary>
public interface IPolicyProvider
{
    /// <summary>
    /// Gets a resilience pipeline for the specified policy type.
    /// </summary>
    /// <param name="policyType">The type of policy to retrieve.</param>
    /// <returns>A resilience pipeline configured for the specified policy type.</returns>
    ResiliencePipeline GetPolicy(PolicyType policyType);
}