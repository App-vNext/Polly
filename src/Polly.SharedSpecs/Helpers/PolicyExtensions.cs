using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Specs.Helpers
{
    public static class PolicyExtensions
    {
        public class ExceptionAndOrCancellationScenario
        {
            public int NumberOfTimesToRaiseException = 0;
            public int? AttemptDuringWhichToCancel = null;
            public bool ActionObservesCancellation = true;
        }

        public static void RaiseException<TException>(this Policy policy, TException instance) where TException : Exception
        {
            ExceptionAndOrCancellationScenario scenario = new ExceptionAndOrCancellationScenario
            {
                ActionObservesCancellation = false,
                AttemptDuringWhichToCancel = null,
                NumberOfTimesToRaiseException = 1
            };

            policy.RaiseExceptionAndOrCancellation(scenario, new CancellationTokenSource(), () => { }, _ => instance);
        }

        public static void RaiseException<TException>(this Policy policy, Action<TException, int> configureException = null) where TException : Exception, new()
        {
            policy.RaiseException(1, configureException);
        }

        public static void RaiseException<TException>(this Policy policy, int numberOfTimesToRaiseException, Action<TException, int> configureException = null) where TException : Exception, new()
        {
            ExceptionAndOrCancellationScenario scenario = new ExceptionAndOrCancellationScenario
            {
                ActionObservesCancellation = false,
                AttemptDuringWhichToCancel = null,
                NumberOfTimesToRaiseException = numberOfTimesToRaiseException
            };

            Func<int, TException> exceptionFactory = i =>
            {
                var exception = new TException();
                configureException?.Invoke(exception, i);
                return exception;
            };

            policy.RaiseExceptionAndOrCancellation(scenario, new CancellationTokenSource(), () => { }, exceptionFactory);
        }

        public static void RaiseExceptionAndOrCancellation<TException>(this Policy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute) where TException : Exception, new()
        {
            policy.RaiseExceptionAndOrCancellation<TException>(scenario, cancellationTokenSource, onExecute, _ => new TException());
        }

        public static TResult RaiseExceptionAndOrCancellation<TException, TResult>(this Policy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute, TResult successResult) where TException : Exception, new()
        {
            return policy.RaiseExceptionAndOrCancellation(scenario, cancellationTokenSource, onExecute,
                _ => new TException(), successResult);
        }

        public static void RaiseExceptionAndOrCancellation<TException>(this Policy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute, Func<int, TException> exceptionFactory) where TException : Exception
        {
            int counter = 0;

            CancellationToken cancellationToken = cancellationTokenSource.Token;

            policy.Execute(ct =>
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
                    throw exceptionFactory(counter);
                }

            }, cancellationToken);
        }

        public static TResult RaiseExceptionAndOrCancellation<TException, TResult>(this Policy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute, Func<int, TException> exceptionFactory, TResult successResult) where TException : Exception
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
                    throw exceptionFactory(counter);
                }

                return successResult;
            }, cancellationToken);
        }
    }
}