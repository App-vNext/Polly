namespace Polly.Utils;

internal static class OutcomeResilienceStrategyHelper<T>
{
    internal static ValueTask<Outcome<TResult>> ConvertValueTask<TResult>(ValueTask<Outcome<T>> valueTask, ResilienceContext resilienceContext)
    {
        if (valueTask.IsCompletedSuccessfully)
        {
            return new ValueTask<Outcome<TResult>>(valueTask.Result.AsOutcome<TResult>());
        }

        return ConvertValueTaskAsync(valueTask, resilienceContext);

        static async ValueTask<Outcome<TResult>> ConvertValueTaskAsync(ValueTask<Outcome<T>> valueTask, ResilienceContext resilienceContext)
        {
            var outcome = await valueTask.ConfigureAwait(resilienceContext.ContinueOnCapturedContext);
            return outcome.AsOutcome<TResult>();
        }
    }
}
