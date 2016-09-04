using System;
using System.Collections.Generic;

namespace Polly.Specs.Helpers
{
    public static class ContextualPolicyExtensions
    {
        public static void RaiseException<TException>(this Policy policy,
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
            this Policy policy,
            IDictionary<string, object> contextData,
            Action<TException, int> configureException = null) where TException : Exception, new()
        {
            policy.RaiseException(1, contextData, configureException);
        }

    }
}