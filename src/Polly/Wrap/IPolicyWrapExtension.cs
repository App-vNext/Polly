namespace Polly.Wrap;

#pragma warning disable CA1062 // Validate arguments of public methods // Temporary stub

/// <summary>
/// Extension methods for IPolicyWrap.
/// </summary>
public static class IPolicyWrapExtension
{
    /// <summary>
    /// Returns all the policies in this <see cref="IPolicyWrap"/>, in Outer-to-Inner order.
    /// </summary>
    /// <param name="policyWrap">The <see cref="IPolicyWrap"/> for which to return policies.</param>
    /// <returns>An <see cref="IEnumerable{IsPolicy}"/> of all the policies in the wrap.</returns>
    public static IEnumerable<IsPolicy> GetPolicies(this IPolicyWrap policyWrap)
    {
        var childPolicies = new[] { policyWrap.Outer, policyWrap.Inner };
        foreach (var childPolicy in childPolicies)
        {
            if (childPolicy is IPolicyWrap anotherWrap)
            {
                foreach (var policy in anotherWrap.GetPolicies())
                {
                    yield return policy;
                }
            }
            else if (childPolicy != null)
            {
                yield return childPolicy;
            }
        }
    }

    /// <summary>
    /// Returns all the policies in this <see cref="IPolicyWrap"/> of type <typeparamref name="TPolicy"/>, in Outer-to-Inner order.
    /// </summary>
    /// <param name="policyWrap">The <see cref="IPolicyWrap"/> for which to return policies.</param>
    /// <typeparam name="TPolicy">The type of policies to return.</typeparam>
    /// <returns>An <see cref="IEnumerable{TPolicy}"/> of all the policies of the given type.</returns>
    public static IEnumerable<TPolicy> GetPolicies<TPolicy>(this IPolicyWrap policyWrap) =>
        policyWrap.GetPolicies().OfType<TPolicy>();

    /// <summary>
    /// Returns all the policies in this <see cref="IPolicyWrap"/> of type <typeparamref name="TPolicy"/> matching the filter, in Outer-to-Inner order.
    /// </summary>
    /// <param name="policyWrap">The <see cref="IPolicyWrap"/> for which to return policies.</param>
    /// <param name="filter">A filter to apply to any policies of type <typeparamref name="TPolicy"/> found.</param>
    /// <typeparam name="TPolicy">The type of policies to return.</typeparam>
    /// <returns>An <see cref="IEnumerable{TPolicy}"/> of all the policies of the given type, matching the filter.</returns>
    public static IEnumerable<TPolicy> GetPolicies<TPolicy>(this IPolicyWrap policyWrap, Func<TPolicy, bool> filter)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        return policyWrap.GetPolicies().OfType<TPolicy>().Where(filter);
    }

    /// <summary>
    /// Returns the single policy in this <see cref="IPolicyWrap"/> of type <typeparamref name="TPolicy"/>.
    /// </summary>
    /// <param name="policyWrap">The <see cref="IPolicyWrap"/> for which to search for the policy.</param>
    /// <typeparam name="TPolicy">The type of policy to return.</typeparam>
    /// <returns>A <typeparamref name="TPolicy"/> if one is found; else null.</returns>
    /// <throws>InvalidOperationException, if more than one policy of the type is found in the wrap.</throws>
    public static TPolicy GetPolicy<TPolicy>(this IPolicyWrap policyWrap) =>
        policyWrap.GetPolicies().OfType<TPolicy>().SingleOrDefault();

    /// <summary>
    /// Returns the single policy in this <see cref="IPolicyWrap"/> of type <typeparamref name="TPolicy"/> matching the filter.
    /// </summary>
    /// <param name="policyWrap">The <see cref="IPolicyWrap"/> for which to search for the policy.</param>
    /// <param name="filter">A filter to apply to any policies of type <typeparamref name="TPolicy"/> found.</param>
    /// <typeparam name="TPolicy">The type of policy to return.</typeparam>
    /// <returns>A matching <typeparamref name="TPolicy"/> if one is found; else null.</returns>
    /// <throws>InvalidOperationException, if more than one policy of the type is found in the wrap.</throws>
    public static TPolicy GetPolicy<TPolicy>(this IPolicyWrap policyWrap, Func<TPolicy, bool> filter)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        return policyWrap.GetPolicies().OfType<TPolicy>().SingleOrDefault(filter);
    }
}
