namespace Polly.Specs.Helpers.Caching;

/// <summary>
/// An intentionally naive class to be the simplest thing possible to support tests around serializing cache providers.  This serialization does nothing but wrap the object to be serialized.
/// </summary>
/// <typeparam name="TOriginal">The type of the item being 'serialized'.</typeparam>
internal class StubSerialized<TOriginal>
{
    public TOriginal? Original;

    public StubSerialized(TOriginal? original) =>
        Original = original;
}

/// <summary>
/// An intentionally naive class to be the simplest thing possible to support tests around serializing cache providers.  This serialization does nothing but wrap the object to be serialized.
/// </summary>
internal class StubSerialized : StubSerialized<object>
{
    public StubSerialized(object? obj)
        : base(obj)
    {
    }
}
