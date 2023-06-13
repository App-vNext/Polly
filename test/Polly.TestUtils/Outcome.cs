namespace Polly.TestUtils;

public static class Outcome
{
    public static Outcome<TResult> AsOutcome<TResult>(this TResult value) => new(value);

    public static Outcome<TResult> AsOutcome<TResult>(this Exception error) => new(error);

    public static ValueTask<Outcome<TResult>> AsOutcomeAsync<TResult>(this TResult value) => AsOutcome(value).AsValueTask();

    public static ValueTask<Outcome<TResult>> AsOutcomeAsync<TResult>(this Exception error) => AsOutcome<TResult>(error).AsValueTask();
}
