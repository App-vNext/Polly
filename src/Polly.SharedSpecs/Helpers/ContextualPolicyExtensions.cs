using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Specs.Helpers
{
    public static class ContextualPolicyExtensions
    {
        public static void RaiseException<TException>(this ContextualPolicy policy,
                   int numberOfTimesToRaiseException,
                   Action<TException, int> configureException = null) where TException : Exception, new()
        {
            int counter = 0;

            policy.Execute(() =>
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
            });
        }

        public static void RaiseException<TException>(
            this ContextualPolicy policy,
            Action<TException, int> configureException = null) where TException : Exception, new()
        {
            policy.RaiseException(1, configureException);
        }

        public static void RaiseException<TException>(this ContextualPolicy policy, 
            int numberOfTimesToRaiseException, 
            IDictionary<string, object> contextData,
            Action<TException, int> configureException = null) where TException : Exception, new()
        {
            int counter = 0;

            policy.Execute(() =>
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
            }, contextData);
        }

        public static void RaiseException<TException>(
            this ContextualPolicy policy,
            IDictionary<string, object> contextData,
            Action<TException, int> configureException = null) where TException : Exception, new()
        {
            policy.RaiseException(1, contextData, configureException);
        }

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