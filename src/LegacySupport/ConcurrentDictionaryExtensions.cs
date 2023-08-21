namespace System.Collections.Concurrent;

#if !NETCOREAPP
internal static class ConcurrentDictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue, TArg>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
    {
        if (dictionary.TryGetValue(key, out TValue value))
        {
            return value;
        }

        return dictionary.GetOrAdd(key, valueFactory(key, factoryArgument));
    }
}
#endif
