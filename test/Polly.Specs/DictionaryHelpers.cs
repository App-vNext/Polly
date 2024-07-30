namespace Polly.Specs;

public static class DictionaryHelpers
{
    public static Dictionary<TKey, object> CreateDictionary<TKey>(TKey key, object value)
        where TKey : notnull => new() { { key, value } };

    public static Dictionary<TKey, object> CreateDictionary<TKey>(TKey key1, object value1, TKey key2, object value2)
        where TKey : notnull => new() { { key1, value1 }, { key2, value2 } };
}
