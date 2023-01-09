namespace Polly;

public struct Outcome<T>
{
    public Outcome(T result) : this()
    {
        Result = result;
    }

    public Outcome(Exception? exception) : this()
    {
        Exception = exception;
        Result = default!;
    }

    public T Result;

    public Exception? Exception { get; }
}
