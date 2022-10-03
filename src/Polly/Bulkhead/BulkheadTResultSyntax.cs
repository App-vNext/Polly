﻿using Polly.Bulkhead;
using System;
using Polly.Bulkhead.Options;

namespace Polly
{
    public partial class Policy
    {
        /// <summary>
        /// <para>Builds a bulkhead isolation <see cref="Policy{TResult}"/>, which limits the maximum concurrency of actions executed through the policy.  Imposing a maximum concurrency limits the potential of governed actions, when faulting, to bring down the system.</para>
        /// <para>When an execution would cause the number of actions executing concurrently through the policy to exceed <paramref name="maxParallelization"/>, the action is not executed and a <see cref="BulkheadRejectedException"/> is thrown.</para>
        /// </summary>
        /// <param name="maxParallelization">The maximum number of concurrent actions that may be executing through the policy.</param>
        /// <exception cref="ArgumentOutOfRangeException">maxParallelization;Value must be greater than zero.</exception>
        /// <returns>The policy instance.</returns>
        public static BulkheadPolicy<TResult> Bulkhead<TResult>(int maxParallelization)
        {
            Action<Context> doNothing = _ => { };
            return Bulkhead<TResult>(maxParallelization, 0, doNothing);
        }

        /// <summary>
        /// <para>Builds a bulkhead isolation <see cref="Policy{TResult}"/>, which limits the maximum concurrency of actions executed through the policy.  Imposing a maximum concurrency limits the potential of governed actions, when faulting, to bring down the system.</para>
        /// <para>When an execution would cause the number of actions executing concurrently through the policy to exceed <paramref name="maxParallelization"/>, the action is not executed and a <see cref="BulkheadRejectedException"/> is thrown.</para>
        /// </summary>
        /// <param name="maxParallelization">The maximum number of concurrent actions that may be executing through the policy.</param>
        /// <param name="onBulkheadRejected">An action to call, if the bulkhead rejects execution due to oversubscription.</param>
        /// <exception cref="ArgumentOutOfRangeException">maxParallelization;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentNullException">onBulkheadRejected</exception>
        /// <returns>The policy instance.</returns>
        public static BulkheadPolicy<TResult> Bulkhead<TResult>(int maxParallelization, Action<Context> onBulkheadRejected)
            => Bulkhead<TResult>(maxParallelization, 0, onBulkheadRejected);

        /// <summary>
        /// Builds a bulkhead isolation <see cref="Policy{TResult}" />, which limits the maximum concurrency of actions executed through the policy.  Imposing a maximum concurrency limits the potential of governed actions, when faulting, to bring down the system.
        /// <para>When an execution would cause the number of actions executing concurrently through the policy to exceed <paramref name="maxParallelization" />, the policy allows a further <paramref name="maxQueuingActions" /> executions to queue, waiting for a concurrent execution slot.  When an execution would cause the number of queuing actions to exceed <paramref name="maxQueuingActions" />, a <see cref="BulkheadRejectedException" /> is thrown.</para>
        /// </summary>
        /// <param name="maxParallelization">The maximum number of concurrent actions that may be executing through the policy.</param>
        /// <param name="maxQueuingActions">The maximum number of actions that may be queuing, waiting for an execution slot.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">maxParallelization;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">maxQueuingActions;Value must be greater than or equal to zero.</exception>
        public static BulkheadPolicy<TResult> Bulkhead<TResult>(int maxParallelization, int maxQueuingActions)
        {
            Action<Context> doNothing = _ => { };
            return Bulkhead<TResult>(maxParallelization, maxQueuingActions, doNothing);
        }

        /// <summary>
        /// Builds a bulkhead isolation <see cref="Policy{TResult}" />, which limits the maximum concurrency of actions executed through the policy.  Imposing a maximum concurrency limits the potential of governed actions, when faulting, to bring down the system.
        /// <para>When an execution would cause the number of actions executing concurrently through the policy to exceed <paramref name="maxParallelization" />, the policy allows a further <paramref name="maxQueuingActions" /> executions to queue, waiting for a concurrent execution slot.  When an execution would cause the number of queuing actions to exceed <paramref name="maxQueuingActions" />, a <see cref="BulkheadRejectedException" /> is thrown.</para>
        /// </summary>
        /// <param name="maxParallelization">The maximum number of concurrent actions that may be executing through the policy.</param>
        /// <param name="maxQueuingActions">The maximum number of actions that may be queuing, waiting for an execution slot.</param>
        /// <param name="onBulkheadRejected">An action to call, if the bulkhead rejects execution due to oversubscription.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">maxParallelization;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">maxQueuingActions;Value must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">onBulkheadRejected</exception>
        public static BulkheadPolicy<TResult> Bulkhead<TResult>(int maxParallelization, int maxQueuingActions, Action<Context> onBulkheadRejected)
        {
            if (maxParallelization <= 0) throw new ArgumentOutOfRangeException(nameof(maxParallelization), "Value must be greater than zero.");
            if (maxQueuingActions < 0) throw new ArgumentOutOfRangeException(nameof(maxQueuingActions), "Value must be greater than or equal to zero.");
            if (onBulkheadRejected is null) throw new ArgumentNullException(nameof(onBulkheadRejected));

            var settings = new BulkheadPolicyOptions(maxParallelization)
            {
                MaxQueuingActions = maxQueuingActions,
                BulkheadRejectedHandler = new DelegateBulkheadRejectionHandler(onBulkheadRejected)
            };
            return Bulkhead<TResult>(settings);
        }
        
        /// <summary>
        /// Builds a bulkhead isolation <see cref="Policy{TResult}" />, which limits the maximum concurrency of actions executed through the policy.  Imposing a maximum concurrency limits the potential of governed actions, when faulting, to bring down the system.
        /// </summary>
        /// <param name="options">The options for the settings of the policy</param>
        /// <returns>The policy instance</returns>
        /// <exception cref="ArgumentNullException">options</exception>
        public static BulkheadPolicy<TResult> Bulkhead<TResult>(BulkheadPolicyOptions options)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));
            return new BulkheadPolicy<TResult>(options);
        }
    }
}
