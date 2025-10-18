namespace Polly.Caching;

/// <summary>
/// Internal wrapper used to avoid JsonElement serialization issues when caching
/// values via untyped (object) resilience pipelines. By wrapping the value into
/// a reference type with a stable shape, the caching layer does not attempt to
/// serialize primitive types into JsonElement, preserving original types for callers.
/// </summary>
internal sealed class CacheObject
{
    public CacheObject()
    {
    }

    public CacheObject(object value)
    {
        Value = value;
        TypeName = value?.GetType().AssemblyQualifiedName;
    }

    public string? TypeName { get; init; }

    public object? Value { get; init; }
}

