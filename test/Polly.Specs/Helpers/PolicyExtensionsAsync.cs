namespace Polly.Specs.Helpers;

public static class PolicyExtensionsAsync
{
    public class ExceptionAndOrCancellationScenario
    {
        public int NumberOfTimesToRaiseException;

        public int? AttemptDuringWhichToCancel;

        public bool ActionObservesCancellation = true;
    }

    public static Task RaiseExceptionAsync<TException>(this AsyncPolicy policy, TException instance)
        where TException : Exception
    {
        ExceptionAndOrCancellationScenario scenario = new ExceptionAndOrCancellationScenario
        {
            ActionObservesCancellation = false,
            AttemptDuringWhichToCancel = null,
            NumberOfTimesToRaiseException = 1
        };

        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        return policy.RaiseExceptionAndOrCancellationAsync(scenario, cancellationTokenSource, () => { }, _ => instance);
    }

    public static Task RaiseExceptionAsync<TException>(this AsyncPolicy policy, Action<TException, int>? configureException = null)
        where TException : Exception, new()
        =>
        policy.RaiseExceptionAsync(1, configureException);

    public static Task RaiseExceptionAsync<TException>(this AsyncPolicy policy, int numberOfTimesToRaiseException, Action<TException, int>? configureException = null)
        where TException : Exception, new()
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

        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        return policy.RaiseExceptionAndOrCancellationAsync(scenario, cancellationTokenSource, () => { }, exceptionFactory);
    }

    public static Task RaiseExceptionAndOrCancellationAsync<TException>(this AsyncPolicy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute)
        where TException : Exception, new()
        =>
        policy.RaiseExceptionAndOrCancellationAsync<TException>(scenario, cancellationTokenSource, onExecute, _ => new TException());

    public static Task<TResult> RaiseExceptionAndOrCancellationAsync<TException, TResult>(this AsyncPolicy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute, TResult successResult)
        where TException : Exception, new()
        =>
        policy.RaiseExceptionAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
            _ => new TException(), successResult);

    public static Task RaiseExceptionAndOrCancellationAsync<TException>(this AsyncPolicy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute, Func<int, TException> exceptionFactory)
        where TException : Exception
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

    public static Task<TResult> RaiseExceptionAndOrCancellationAsync<TException, TResult>(this AsyncPolicy policy, ExceptionAndOrCancellationScenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute, Func<int, TException> exceptionFactory, TResult successResult)
        where TException : Exception
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
