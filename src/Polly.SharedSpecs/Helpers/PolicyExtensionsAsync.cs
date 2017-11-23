using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Specs.Helpers
{
    public static class PolicyExtensionsAsync
    {
        public class ExceptionAndOrCancellationScenario
        {
            public int NumberOfTimesToRaiseException = 0;

            public int? AttemptDuringWhichToCancel = null;

            public bool ActionObservesCancellation = true;
        }

        public static Task RaiseExceptionAsync<TException>(this Policy policy, TException instance) where TException : Exception
        {
            ExceptionAndOrCancellationScenario scenario = new ExceptionAndOrCancellationScenario
            {
                ActionObservesCancellation = false,
                AttemptDuringWhichToCancel = null,
                NumberOfTimesToRaiseException = 1
            };

            return policy.RaiseExceptionAndOrCancellationAsync(scenario, new CancellationTokenSource(), () => { }, _ => instance);
        }

        public static Task RaiseExceptionAsync<TException>(this Policy policy, Action<TException, int> configureException = null) where TException : Exception, new()
        {
            return policy.RaiseExceptionAsync(1, configureException);
        }

        public static Task RaiseExceptionAsync<TException>(this Policy policy, int numberOfTimesToRaiseException, Action<TException, int> configureException = null, CancellationToken cancellationToken = default(CancellationToken)) where TException : Exception, new()
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

            return policy.RaiseExceptionAndOrCancellationAsync(scenario, new CancellationTokenSource(), () => { }, exceptionFactory);
        }

        public static Task RaiseExceptionAndOrCancellationAsync<TException>(this Policy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute) where TException : Exception, new()
        {
            return policy.RaiseExceptionAndOrCancellationAsync<TException>(scenario, cancellationTokenSource, onExecute, _ => new TException());
        }

        public static Task<TResult> RaiseExceptionAndOrCancellationAsync<TException, TResult>(this Policy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute, TResult successResult) where TException : Exception, new()
        {
            return policy.RaiseExceptionAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                _ => new TException(), successResult);
        }

        public static Task RaiseExceptionAndOrCancellationAsync<TException>(this Policy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute, Func<int, TException> exceptionFactory) where TException : Exception
        {
            int counter = 0;

            CancellationToken cancellationToken = cancellationTokenSource.Token;

            return policy.ExecuteAsync(ct =>
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

                return TaskHelper.EmptyTask;
            }, cancellationToken);
        }

        public static Task<TResult> RaiseExceptionAndOrCancellationAsync<TException, TResult>(this Policy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute, Func<int, TException> exceptionFactory, TResult successResult) where TException : Exception
        {
            int counter = 0;

            CancellationToken cancellationToken = cancellationTokenSource.Token;

            return policy.ExecuteAsync(ct =>
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

                return Task.FromResult(successResult);
            }, cancellationToken);
        }
    }
}