using Polly.Utils;

namespace Polly.Telemetry;

internal sealed class TagsList
{
    private const int InitialArraySize = 20;

    private static readonly ObjectPool<TagsList> ContextPool = new(static () => new TagsList(), static context =>
    {
        context.Tags.Clear();
        return true;
    });

    private KeyValuePair<string, object?>[] _tagsArray = new KeyValuePair<string, object?>[InitialArraySize];

    private TagsList()
    {
    }

    internal static TagsList Get()
    {
        var context = ContextPool.Get();

        return context;
    }

    internal static void Return(TagsList context)
    {
        Array.Clear(context._tagsArray, 0, context.Tags.Count);
        context.Tags.Clear();
        ContextPool.Return(context);
    }

    /// <summary>
    /// Gets the tags associated with the resilience event.
    /// </summary>
    public IList<KeyValuePair<string, object?>> Tags { get; } = [];

    internal ReadOnlySpan<KeyValuePair<string, object?>> TagsSpan
    {
        get
        {
            // stryker disable once equality : no means to test this
            if (Tags.Count > _tagsArray.Length)
            {
                Array.Resize(ref _tagsArray, Tags.Count);
            }

            for (int i = 0; i < Tags.Count; i++)
            {
                _tagsArray[i] = Tags[i];
            }

            return _tagsArray.AsSpan(0, Tags.Count);
        }
    }
}
