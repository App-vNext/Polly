using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Specs.Helpers
{
    public static class IAsyncPolicyExtensions
    {
        public class ExceptionAndOrCancellationScenario
        {
            public int NumberOfTimesToRaiseException;

            public int? AttemptDuringWhichToCancel;

            public bool ActionObservesCancellation = true;
        }

        public static Task RaiseExceptionAsync<TException>(this IAsyncPolicy policy, TException instance) where TException : Exception
        {
            ExceptionAndOrCancellationScenario scenario = new ExceptionAndOrCancellationScenario
            {
                ActionObservesCancellation = false,
                AttemptDuringWhichToCancel = null,
                NumberOfTimesToRaiseException = 1
            };

            return policy.RaiseExceptionAndOrCancellationAsync(scenario, new CancellationTokenSource(), () => { }, _ => instance);
        }

        public static Task RaiseExceptionAsync<TException>(this IAsyncPolicy policy, Action<TException, int> configureException = null) where TException : Exception, new()
        {
            return policy.RaiseExceptionAsync(1, configureException);
        }

        public static Task RaiseExceptionAsync<TException>(this IAsyncPolicy policy, int numberOfTimesToRaiseException, Action<TException, int> configureException = null, CancellationToken cancellationToken = default) where TException : Exception, new()
        {
            return policy.RaiseExceptionAsync(numberOfTimesToRaiseException, new Dictionary<string, object>(0), configureException, cancellationToken);
        }

        public static Task RaiseExceptionAsync<TException>(this IAsyncPolicy policy, int numberOfTimesToRaiseException, IDictionary<string, object> contextData, Action<TException, int> configureException = null, CancellationToken cancellationToken = default) where TException : Exception, new()
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

            return policy.RaiseExceptionAndOrCancellationAsync(scenario, contextData, new CancellationTokenSource(), () => { }, exceptionFactory);
        }

        public static Task RaiseExceptionAsync<TException>(this IAsyncPolicy policy, IDictionary<string, object> contextData, Action<TException, int> configureException = null, CancellationToken cancellationToken = default) where TException : Exception, new()
        {
            return policy.RaiseExceptionAsync(1, contextData, configureException, cancellationToken);
        }

        public static Task RaiseExceptionAndOrCancellationAsync<TException>(this IAsyncPolicy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute) where TException : Exception, new()
        {
            return policy.RaiseExceptionAndOrCancellationAsync<TException>(scenario, cancellationTokenSource, onExecute, _ => new TException());
        }

        public static Task<TResult> RaiseExceptionAndOrCancellationAsync<TException, TResult>(this IAsyncPolicy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute, TResult successResult) where TException : Exception, new()
        {
            return policy.RaiseExceptionAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                _ => new TException(), successResult);
        }

        public static Task RaiseExceptionAndOrCancellationAsync<TException>(
            this IAsyncPolicy policy,
            ExceptionAndOrCancellationScenario scenario,
            CancellationTokenSource cancellationTokenSource,
            Action onExecute,
            Func<int, TException> exceptionFactory) where TException : Exception
        {
            return policy.RaiseExceptionAndOrCancellationAsync(
                scenario,
                new Dictionary<string, object>(0),
                cancellationTokenSource,
                onExecute,
                exceptionFactory);
        }

        public static Task RaiseExceptionAndOrCancellationAsync<TException>(
            this IAsyncPolicy policy,
            ExceptionAndOrCancellationScenario scenario,
            IDictionary<string, object> contextData,
            CancellationTokenSource cancellationTokenSource,
            Action onExecute,
            Func<int, TException> exceptionFactory) where TException : Exception
        {
            int counter = 0;

            CancellationToken cancellationToken = cancellationTokenSource.Token;

            return policy.ExecuteAsync((ctx, ct) =>
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
            }, contextData, cancellationToken);
        }

        public static Task<TResult> RaiseExceptionAndOrCancellationAsync<TException, TResult>(
            this IAsyncPolicy policy,
            ExceptionAndOrCancellationScenario scenario,
            CancellationTokenSource cancellationTokenSource,
            Action onExecute,
            Func<int, TException> exceptionFactory,
            TResult successResult)
            where TException : Exception
        {
            return policy.RaiseExceptionAndOrCancellationAsync(
                scenario,
                new Dictionary<string, object>(0),
                cancellationTokenSource,
                onExecute,
                exceptionFactory,
                successResult);
        }

        public static Task<TResult> RaiseExceptionAndOrCancellationAsync<TException, TResult>(
            this IAsyncPolicy policy,
            ExceptionAndOrCancellationScenario scenario,
            IDictionary<string, object> contextData,
            CancellationTokenSource cancellationTokenSource,
            Action onExecute,
            Func<int, TException> exceptionFactory,
            TResult successResult) where TException : Exception
        {
            int counter = 0;

            CancellationToken cancellationToken = cancellationTokenSource.Token;

            return policy.ExecuteAsync((ctx, ct) =>
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
            }, contextData, cancellationToken);
        }
    }
}