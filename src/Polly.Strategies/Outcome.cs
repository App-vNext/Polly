namespace Polly;

public readonly struct Outcome<T>
{
    public Outcome(T result) : this() => Result = result;

    public Outcome(Exception? exception) : this()
    {
        Exception = exception;
        Result = default!;
    }

    public T Result { get; }

    public Exception? Exception { get; }
}
