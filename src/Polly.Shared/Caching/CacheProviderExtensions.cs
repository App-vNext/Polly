namespace Polly.Caching
{
    /// <summary>
    /// Class that provides helper methods for configuring CacheProviders.
    /// </summary>
    public static class CacheProviderExtensions
    {
        /// <summary>
        /// Provides a strongly <typeparamref name="TCacheFormat"/>-typed version of the supplied <see cref="ICacheProvider"/>
        /// </summary>
        /// <typeparam name="TCacheFormat">The type the returned <see cref="ICacheProvider{TResult}"/> will handle.</typeparam>
        /// <param name="nonGenericCacheProvider">The non-generic cache provider to wrap.</param>
        /// <returns>ICacheProvider{TCacheFormat}.</returns>
        public static ICacheProvider<TCacheFormat> As<TCacheFormat>(this ICacheProvider nonGenericCacheProvider)
        {
            return new TypedCacheProvider<TCacheFormat>(nonGenericCacheProvider);
        }

        /// <summary>
        /// Provides a strongly <typeparamref name="TCacheFormat"/>-typed version of the supplied <see cref="ICacheProviderAsync"/>
        /// </summary>
        /// <typeparam name="TCacheFormat">The type the returned <see cref="ICacheProvider{TResult}"/> will handle.</typeparam>
        /// <param name="nonGenericCacheProvider">The non-generic cache provider to wrap.</param>
        /// <returns>ICacheProviderAsync{TCacheFormat}.</returns>
        public static ICacheProviderAsync<TCacheFormat> AsyncAs<TCacheFormat>(this ICacheProviderAsync nonGenericCacheProvider)
        {
            return new TypedCacheProviderAsync<TCacheFormat>(nonGenericCacheProvider);
        }

        /// <summary>
        /// Wraps the <paramref name="serializer"/> around the <paramref name="cacheProvider"/> so that delegate return values of any type can be stored in the cache as type <typeparamref name="TSerialized"/>.
        /// </summary>
        /// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="serializer">A serializer which can serialize/deserialize all types to/from <typeparamref name="TSerialized"/>.</param>
        /// <returns>SerializingCacheProvider&lt;TResult, TSerialized&gt;.</returns>
        public static SerializingCacheProvider<TSerialized> WithSerializer<TSerialized>(
            this ICacheProvider<TSerialized> cacheProvider, ICacheItemSerializer<object, TSerialized> serializer)
        {
            return new SerializingCacheProvider<TSerialized>(cacheProvider, serializer);
        }

        /// <summary>
        /// Wraps the <paramref name="serializer"/> around the <paramref name="cacheProvider"/> so that delegate return values of type <typeparamref name="TResult"/> can be stored in the cache as type <typeparamref name="TSerialized"/>.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns>SerializingCacheProvider&lt;TResult, TSerialized&gt;.</returns>
        public static SerializingCacheProvider<TResult, TSerialized> WithSerializer<TResult, TSerialized>(
            this ICacheProvider<TSerialized> cacheProvider, ICacheItemSerializer<TResult, TSerialized> serializer)
        {
            return new SerializingCacheProvider<TResult, TSerialized>(cacheProvider, serializer);
        }

        /// <summary>
        /// Wraps the <paramref name="serializer"/> around the asynchronous <paramref name="cacheProvider"/> so that delegate return values of any type can be stored in the cache as type <typeparamref name="TSerialized"/>.
        /// </summary>
        /// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="serializer">A serializer which can serialize/deserialize all types to/from <typeparamref name="TSerialized"/>.</param>
        /// <returns>SerializingCacheProvider&lt;TResult, TSerialized&gt;.</returns>
        public static SerializingCacheProviderAsync<TSerialized> WithSerializer<TSerialized>(
            this ICacheProviderAsync<TSerialized> cacheProvider, ICacheItemSerializer<object, TSerialized> serializer)
        {
            return new SerializingCacheProviderAsync<TSerialized>(cacheProvider, serializer);
        }

        /// <summary>
        /// Wraps the <paramref name="serializer"/> around the asynchronous <paramref name="cacheProvider"/> so that delegate return values of type <typeparamref name="TResult"/> can be stored in the cache as type <typeparamref name="TSerialized"/>.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns>SerializingCacheProvider&lt;TResult, TSerialized&gt;.</returns>
        public static SerializingCacheProviderAsync<TResult, TSerialized> WithSerializer<TResult, TSerialized>(
            this ICacheProviderAsync<TSerialized> cacheProvider, ICacheItemSerializer<TResult, TSerialized> serializer)
        {
            return new SerializingCacheProviderAsync<TResult, TSerialized>(cacheProvider, serializer);
        }
    }
}
