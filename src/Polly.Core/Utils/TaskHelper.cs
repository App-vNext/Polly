namespace Polly.Utils;

internal static class TaskHelper
{
    public static ValueTask<Outcome<TTo>> ConvertValueTask<TFrom, TTo>(ValueTask<Outcome<TFrom>> valueTask, ResilienceContext resilienceContext)
    {
        if (valueTask.IsCompletedSuccessfully)
        {
            return new ValueTask<Outcome<TTo>>(valueTask.Result.AsOutcome<TTo>());
        }

        return ConvertValueTaskAsync(valueTask, resilienceContext);

        static async ValueTask<Outcome<TTo>> ConvertValueTaskAsync(ValueTask<Outcome<TFrom>> valueTask, ResilienceContext resilienceContext)
        {
            var outcome = await valueTask.ConfigureAwait(resilienceContext.ContinueOnCapturedContext);
            return outcome.AsOutcome<TTo>();
        }
    }
}
