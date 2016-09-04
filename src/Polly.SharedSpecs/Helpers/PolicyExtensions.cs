using System;
using System.Threading;
using System.Threading.Tasks;

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


        public class ExceptionAndOrCancellationScenario
        {
            public int NumberOfTimesToRaiseException = 0;

            public int? AttemptDuringWhichToCancel = null;

            public bool ActionObservesCancellation = true;
        }

        public static void RaiseExceptionAndOrCancellation<TException>(this Policy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute) where TException : Exception, new()
        {
            policy.RaiseExceptionAndOrCancellation<TException, int>(scenario, cancellationTokenSource, onExecute, 0);
        }

        public static TResult RaiseExceptionAndOrCancellation<TException, TResult>(this Policy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute, TResult successResult) where TException : Exception, new()
        {
            int counter = 0;

            CancellationToken cancellationToken = cancellationTokenSource.Token;

            return policy.Execute(ct =>
            {
                onExecute();

                counter++;

                if (scenario.AttemptDuringWhichToCancel.HasValue && counter >= scenario.AttemptDuringWhichToCancel.Value)
                {
                    cancellationTokenSource.Cancel();
                }

                if (scenario.ActionObservesCancellation)
                {
                    ct.ThrowIfCancellationRequested();
                }

                if (counter <= scenario.NumberOfTimesToRaiseException)
                {
                    throw new TException();
                }

                return successResult;
            }, cancellationToken);
        }
    }
}