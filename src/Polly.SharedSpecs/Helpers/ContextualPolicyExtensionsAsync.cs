using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Specs.Helpers
{
    public static class ContextualPolicyExtensionsAsync
    {

        public static Task RaiseExceptionAsync<TException>(this ContextualPolicy policy, int numberOfTimesToRaiseException, IDictionary<string, object> contextData, Action<TException, int> configureException = null, CancellationToken cancellationToken = default(CancellationToken)) where TException : Exception, new()
        {
            int counter = 0;

            return policy.ExecuteAsync(ct =>
            {
                if (counter < numberOfTimesToRaiseException)
                {
                    counter++;

                    var exception = new TException();

                    if (configureException != null)
                    {
                        configureException(exception, counter);
                    }

                    throw exception;
                }
                return Task.FromResult(true) as Task;
            }, contextData, cancellationToken);
        }

        public static Task RaiseExceptionAsync<TException>(this ContextualPolicy policy, IDictionary<string, object> contextData, Action<TException, int> configureException = null, CancellationToken cancellationToken = default(CancellationToken)) where TException : Exception, new()
        {
            return policy.RaiseExceptionAsync(1, contextData, configureException, cancellationToken);
        }

    }
}