using System;

namespace Polly.Specs.Helpers
{
    public static class PolicyExtensions
    {
        public static void RaiseException<TException>(this Policy policy, int numberOfTimesToRaiseException, Action<TException, int> configureException = null) where TException : Exception, new()
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

        public static void RaiseException<TException>(this Policy policy, Action<TException, int> configureException = null) where TException : Exception, new()
        {
            policy.RaiseException(1, configureException);
        }
    }
}