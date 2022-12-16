using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Specs.Helpers
{
    public static class ContextualPolicyExtensionsAsync
    {

        public static Task RaiseExceptionAsync<TException>(this AsyncPolicy policy, int numberOfTimesToRaiseException, IDictionary<string, object> contextData, Action<TException, int> configureException = null, CancellationToken cancellationToken = default) where TException : Exception, new()
        {
            int counter = 0;

            return policy.ExecuteAsync((_, _) =>
            {
                if (counter < numberOfTimesToRaiseException)
                {
                    counter++;

                    var exception = new TException();

                    configureException?.Invoke(exception, counter);

                    throw exception;
                }
                return TaskHelper.EmptyTask;
            }, contextData, cancellationToken);
        }

        public static Task RaiseExceptionAsync<TException>(this AsyncPolicy policy, IDictionary<string, object> contextData, Action<TException, int> configureException = null, CancellationToken cancellationToken = default) where TException : Exception, new()
        {
            return policy.RaiseExceptionAsync(1, contextData, configureException, cancellationToken);
        }

    }
}