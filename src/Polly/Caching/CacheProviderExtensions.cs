#nullable enable

namespace Polly.Caching;

/// <summary>
/// Class that provides helper methods for configuring CacheProviders.
/// </summary>
public static class CacheProviderExtensions
{
    /// <summary>
    /// Provides a strongly <typeparamref name="TCacheFormat"/>-typed version of the supplied <see cref="ISyncCacheProvider"/>.
    /// </summary>
    /// <typeparam name="TCacheFormat">The type the returned <see cref="ISyncCacheProvider{TResult}"/> will handle.</typeparam>
    /// <param name="nonGenericCacheProvider">The non-generic cache provider to wrap.</param>
    /// <returns>ISyncCacheProvider{TCacheFormat}.</returns>
    public static ISyncCacheProvider<TCacheFormat> For<TCacheFormat>(this ISyncCacheProvider nonGenericCacheProvider) =>
        new GenericCacheProvider<TCacheFormat>(nonGenericCacheProvider);

    /// <summary>
    /// Provides a strongly <typeparamref name="TCacheFormat"/>-typed version of the supplied <see cref="IAsyncCacheProvider"/>.
    /// </summary>
    /// <typeparam name="TCacheFormat">The type the returned <see cref="IAsyncCacheProvider{TResult}"/> will handle.</typeparam>
    /// <param name="nonGenericCacheProvider">The non-generic cache provider to wrap.</param>
    /// <returns>IAsyncCacheProvider{TCacheFormat}.</returns>
    public static IAsyncCacheProvider<TCacheFormat> AsyncFor<TCacheFormat>(this IAsyncCacheProvider nonGenericCacheProvider) =>
        new AsyncGenericCacheProvider<TCacheFormat>(nonGenericCacheProvider);

    /// <summary>
    /// Wraps the <paramref name="serializer"/> around the <paramref name="cacheProvider"/> so that delegate return values of any type can be stored in the cache as type <typeparamref name="TSerialized"/>.
    /// </summary>
    /// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="serializer">A serializer which can serialize/deserialize all types to/from <typeparamref name="TSerialized"/>.</param>
    /// <returns>SerializingCacheProvider&lt;TResult, TSerialized&gt;.</returns>
    public static SerializingCacheProvider<TSerialized> WithSerializer<TSerialized>(
        this ISyncCacheProvider<TSerialized> cacheProvider, ICacheItemSerializer<object, TSerialized> serializer) =>
        new(cacheProvider, serializer);

    /// <summary>
    /// Wraps the <paramref name="serializer"/> around the <paramref name="cacheProvider"/> so that delegate return values of type <typeparamref name="TResult"/> can be stored in the cache as type <typeparamref name="TSerialized"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    /// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="serializer">The serializer.</param>
    /// <returns>SerializingCacheProvider&lt;TResult, TSerialized&gt;.</returns>
    public static SerializingCacheProvider<TResult, TSerialized> WithSerializer<TResult, TSerialized>(
        this ISyncCacheProvider<TSerialized> cacheProvider, ICacheItemSerializer<TResult, TSerialized> serializer) =>
        new(cacheProvider, serializer);

    /// <summary>
    /// Wraps the <paramref name="serializer"/> around the asynchronous <paramref name="cacheProvider"/> so that delegate return values of any type can be stored in the cache as type <typeparamref name="TSerialized"/>.
    /// </summary>
    /// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="serializer">A serializer which can serialize/deserialize all types to/from <typeparamref name="TSerialized"/>.</param>
    /// <returns>SerializingCacheProvider&lt;TResult, TSerialized&gt;.</returns>
    public static AsyncSerializingCacheProvider<TSerialized> WithSerializer<TSerialized>(
        this IAsyncCacheProvider<TSerialized> cacheProvider, ICacheItemSerializer<object, TSerialized> serializer) =>
        new(cacheProvider, serializer);

    /// <summary>
    /// Wraps the <paramref name="serializer"/> around the asynchronous <paramref name="cacheProvider"/> so that delegate return values of type <typeparamref name="TResult"/> can be stored in the cache as type <typeparamref name="TSerialized"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    /// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="serializer">The serializer.</param>
    /// <returns>SerializingCacheProvider&lt;TResult, TSerialized&gt;.</returns>
    public static AsyncSerializingCacheProvider<TResult, TSerialized> WithSerializer<TResult, TSerialized>(
        this IAsyncCacheProvider<TSerialized> cacheProvider, ICacheItemSerializer<TResult, TSerialized> serializer) =>
        new(cacheProvider, serializer);
}
