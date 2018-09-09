using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Specs.Helpers
{
    public static class ContextualPolicyExtensionsAsync
    {

        public static Task RaiseExceptionAsync<TException>(this Policy policy, int numberOfTimesToRaiseException, IDictionary<string, object> contextData, Action<TException, int> configureException = null, CancellationToken cancellationToken = default(CancellationToken)) where TException : Exception, new()
        {
            var counter = 0;

            return policy.ExecuteAsync((ctx, ct) =>
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
                return Task.CompletedTask;
            }, contextData, cancellationToken);
        }

        public static Task RaiseExceptionAsync<TException>(this Policy policy, IDictionary<string, object> contextData, Action<TException, int> configureException = null, CancellationToken cancellationToken = default(CancellationToken)) where TException : Exception, new() => policy.RaiseExceptionAsync(1, contextData, configureException, cancellationToken);

    }
}