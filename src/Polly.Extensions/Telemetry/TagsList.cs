#if NET
using System.Runtime.InteropServices;
#endif
using Polly.Utils;

namespace Polly.Telemetry;

internal sealed class TagsList
{
    private static readonly ObjectPool<TagsList> ContextPool = new(static () => new TagsList(), static _ => true);

#if !NET
    private KeyValuePair<string, object?>[] _tagsArray = new KeyValuePair<string, object?>[20];
#endif

    private TagsList()
    {
    }

    internal static TagsList Get() => ContextPool.Get();

    internal static void Return(TagsList context)
    {
#if !NET
        Array.Clear(context._tagsArray, 0, context.Tags.Count);
#endif
        context.Tags.Clear();
        ContextPool.Return(context);
    }

    /// <summary>
    /// Gets the tags associated with the resilience event.
    /// </summary>
    public List<KeyValuePair<string, object?>> Tags { get; } = [];

    internal ReadOnlySpan<KeyValuePair<string, object?>> TagsSpan
    {
        get
        {
#if NET
            return CollectionsMarshal.AsSpan(Tags);
#else
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
#endif
        }
    }
}
