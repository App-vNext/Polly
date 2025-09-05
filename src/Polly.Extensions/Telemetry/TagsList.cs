#if NET
using System.Runtime.InteropServices;
#endif
using Polly.Utils;

namespace Polly.Telemetry;

internal sealed class TagsList : List<KeyValuePair<string, object?>>
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
        Array.Clear(context._tagsArray, 0, context.Count);
#endif
        context.Clear();
        ContextPool.Return(context);
    }

    internal ReadOnlySpan<KeyValuePair<string, object?>> TagsSpan
    {
        get
        {
#if NET
            return CollectionsMarshal.AsSpan(this);
#else
            // stryker disable once equality : no means to test this
            if (Count > _tagsArray.Length)
            {
                Array.Resize(ref _tagsArray, Count);
            }

            CopyTo(_tagsArray, 0);
            return _tagsArray.AsSpan(0, Count);
#endif
        }
    }
}
