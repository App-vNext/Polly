using System;
using System.Collections.Generic;
using System.Threading;

namespace Polly.Specs.Helpers
{
    public static class PolicyExtensions
    {
        public class ExceptionAndOrCancellationScenario
        {
            public int NumberOfTimesToRaiseException;
            public int? AttemptDuringWhichToCancel;
            public bool ActionObservesCancellation = true;
        }

        public static void RaiseException<TException>(this ISyncPolicy policy, TException instance) where TException : Exception
        {
            ExceptionAndOrCancellationScenario scenario = new ExceptionAndOrCancellationScenario
            {
                ActionObservesCancellation = false,
                AttemptDuringWhichToCancel = null,
                NumberOfTimesToRaiseException = 1
            };

            policy.RaiseExceptionAndOrCancellation(scenario, new CancellationTokenSource(), () => { }, _ => instance);
        }

        public static void RaiseException<TException>(this ISyncPolicy policy, Action<TException, int> configureException = null) where TException : Exception, new()
        {
            policy.RaiseException(1, configureException);
        }

        public static void RaiseException<TException>(this ISyncPolicy policy, int numberOfTimesToRaiseException, Action<TException, int> configureException = null) where TException : Exception, new()
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

        public static void RaiseException<TException>(this ISyncPolicy policy,
            int numberOfTimesToRaiseException,
            IDictionary<string, object> contextData) where TException : Exception, new()
        {
            ExceptionAndOrCancellationScenario scenario = new ExceptionAndOrCancellationScenario
            {
                ActionObservesCancellation = false,
                AttemptDuringWhichToCancel = null,
                NumberOfTimesToRaiseException = numberOfTimesToRaiseException
            };

            Func<int, TException> exceptionFactory = i => new TException();

            policy.RaiseExceptionAndOrCancellation(scenario, contextData, new CancellationTokenSource(), () => { }, exceptionFactory);
        }

        public static void RaiseException<TException>(
            this ISyncPolicy policy,
            IDictionary<string, object> contextData) where TException : Exception, new()
        {
            policy.RaiseException<TException>(1, contextData);
        }

        public static void RaiseExceptionAndOrCancellation<TException>(this ISyncPolicy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute) where TException : Exception, new()
        {
            policy.RaiseExceptionAndOrCancellation<TException>(scenario, cancellationTokenSource, onExecute, _ => new TException());
        }

        public static TResult RaiseExceptionAndOrCancellation<TException, TResult>(this ISyncPolicy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute, TResult successResult) where TException : Exception, new()
        {
            return policy.RaiseExceptionAndOrCancellation(scenario, cancellationTokenSource, onExecute,
                _ => new TException(), successResult);
        }

        public static void RaiseExceptionAndOrCancellation<TException>(
            this ISyncPolicy policy,
            ExceptionAndOrCancellationScenario scenario,
            CancellationTokenSource cancellationTokenSource,
            Action onExecute,
            Func<int, TException> exceptionFactory) where TException : Exception
        {
            policy.RaiseExceptionAndOrCancellation(
                scenario,
                new Dictionary<string, object>(0),
                cancellationTokenSource,
                onExecute,
                exceptionFactory);
        }

        public static void RaiseExceptionAndOrCancellation<TException>(
            this ISyncPolicy policy,
            ExceptionAndOrCancellationScenario scenario,
            IDictionary<string, object> contextData,
            CancellationTokenSource cancellationTokenSource,
            Action onExecute,
            Func<int, TException> exceptionFactory) where TException : Exception
        {
            int counter = 0;

            CancellationToken cancellationToken = cancellationTokenSource.Token;

            policy.Execute((ctx, ct) =>
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

            }, contextData, cancellationToken);
        }


        public static TResult RaiseExceptionAndOrCancellation<TException, TResult>(
            this ISyncPolicy policy,
            ExceptionAndOrCancellationScenario scenario,
            CancellationTokenSource cancellationTokenSource,
            Action onExecute,
            Func<int, TException> exceptionFactory,
            TResult successResult)
            where TException : Exception
        {
            return policy.RaiseExceptionAndOrCancellation(
                scenario,
                new Dictionary<string, object>(0),
                cancellationTokenSource,
                onExecute,
                exceptionFactory,
                successResult);
        }

        public static TResult RaiseExceptionAndOrCancellation<TException, TResult>(
            this ISyncPolicy policy,
            ExceptionAndOrCancellationScenario scenario,
            IDictionary<string, object> contextData,
            CancellationTokenSource cancellationTokenSource,
            Action onExecute,
            Func<int, TException> exceptionFactory,
            TResult successResult) where TException : Exception
        {
            int counter = 0;

            CancellationToken cancellationToken = cancellationTokenSource.Token;

            return policy.Execute((ctx, ct) =>
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
            }, contextData, cancellationToken);
        }
    }
}