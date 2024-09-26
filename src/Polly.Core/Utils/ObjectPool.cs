namespace Polly.Utils;

// copied from https://raw.githubusercontent.com/dotnet/aspnetcore/53124ab8cbf152f59120982f1c74e802e7970845/src/ObjectPool/src/DefaultObjectPool.cs
internal sealed class ObjectPool<T>
    where T : class
{
    internal static readonly int MaxCapacity = (Environment.ProcessorCount * 2) - 1; // the - 1 is to account for _fastItem

    private readonly Func<T> _createFunc;
    private readonly Func<T, bool> _returnFunc;

    private readonly ConcurrentQueue<T> _items = new();

    private T? _fastItem;
    private int _numItems;

    public ObjectPool(Func<T> createFunc, Func<T, bool> returnFunc)
    {
        _createFunc = createFunc;
        _returnFunc = returnFunc;
    }

    public T Get()
    {
        var item = _fastItem;
        if (item == null || Interlocked.CompareExchange(ref _fastItem, null, item) != item)
        {
            if (_items.TryDequeue(out item))
            {
                Interlocked.Decrement(ref _numItems);
                return item;
            }

            // no object available, so go get a brand new one
            return _createFunc();
        }

        return item;
    }

    public void Return(T obj)
    {
        if (!_returnFunc(obj))
        {
            // policy says to drop this object
            return;
        }

        // Stryker disable once equality : no means to test this
        if (_fastItem != null || Interlocked.CompareExchange(ref _fastItem, obj, null) != null)
        {
            // Stryker disable once equality : no means to test this
            if (Interlocked.Increment(ref _numItems) <= MaxCapacity)
            {
                _items.Enqueue(obj);
                return;
            }

            // no room, clean up the count and drop the object on the floor
            Interlocked.Decrement(ref _numItems);
        }
    }
}
