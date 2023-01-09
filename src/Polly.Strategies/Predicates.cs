namespace Polly;

public delegate ValueTask<bool> PredicatesDelegate<T, TContext>(Outcome<T> outcome, TContext context);

public class Predicates<TContext>
{
    internal Dictionary<Type, List<object>> Callbacks { get; } = new();

    public Predicates<TContext> AddException<T>()
        where T : Exception
    {
        return Add<T>((o) => o.Exception is T);
    }

    public Predicates<TContext> AddException<T>(Func<T, bool> predicate)
        where T : Exception
    {
        return Add<T>((o) => o.Exception is T exception && predicate(exception));
    }

    public Predicates<TContext> Add<T>(T value)
    {
        return Add<T>((o) => o.Exception == null && EqualityComparer<T>.Default.Equals(o.Result, value));
    }

    public Predicates<TContext> Add<T>(Func<T, bool> predicate)
    {
        return Add<T>((o) => o.Exception == null && predicate(o.Result));
    }

    public Predicates<TContext> Add<T>(Func<Outcome<T>, bool> predicate)
    {
        return Add<T>((o, _) => predicate(o));
    }

    public Predicates<TContext> Add<T>(Func<Outcome<T>, TContext, bool> predicate)
    {
        return Add<T>((o, c) => new ValueTask<bool>(predicate(o, c)));
    }

    public Predicates<TContext> Add<T>(PredicatesDelegate<T, TContext> predicate)
    {
        var type = typeof(T);

        if (Callbacks.TryGetValue(type, out var list))
        {
            list.Add(predicate);
        }
        else
        {
            Callbacks.Add(type, new List<object> { predicate });
        }

        return this;
    }
}

