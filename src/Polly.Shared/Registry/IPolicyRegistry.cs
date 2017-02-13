using System.Collections.Generic;

namespace Polly.Registry
{
    /// <summary>
    /// Represents a collection of <see cref="Polly.Policy"/> keyed by <typeparamref name="Key"/>
    /// </summary>
    /// <typeparam name="Key">The type of keys in the dictionary</typeparam>
    /// <typeparam name="Policy">The type of Policy to store. Must have <see cref="Polly.Policy"/> as base class. </typeparam>
    public interface IPolicyRegistry<Key, Policy> : IDictionary<Key, Policy> where Policy: Polly.Policy
    {
    }
}
