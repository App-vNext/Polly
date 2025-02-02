namespace Polly.Specs.Caching;

public class AsyncSerializingCacheProviderSpecs
{
    #region Object-to-TSerialized serializer

    private static CancellationToken CancellationToken => CancellationToken.None;

    [Fact]
    public void Single_generic_constructor_should_throw_on_no_wrapped_cache_provider()
    {
        StubSerializer<object, StubSerialized> stubObjectSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => new StubSerialized(o),
            deserialize: s => s?.Original ?? default);

        Action configure = () => _ = new AsyncSerializingCacheProvider<StubSerialized>(null!, stubObjectSerializer);

        Should.Throw<ArgumentNullException>(configure)
            .ParamName.ShouldBe("wrappedCacheProvider");
    }

    [Fact]
    public void Single_generic_constructor_should_throw_on_no_serializer()
    {
        Action configure = () => _ = new AsyncSerializingCacheProvider<object>(new StubCacheProvider().AsyncFor<object>(), null!);

        Should.Throw<ArgumentNullException>(configure)
            .ParamName.ShouldBe("serializer");
    }

    [Fact]
    public void Single_generic_extension_syntax_should_throw_on_no_serializer()
    {
        Action configure = () => new StubCacheProvider().AsyncFor<object>().WithSerializer(null!);

        Should.Throw<ArgumentNullException>(configure)
            .ParamName.ShouldBe("serializer");
    }

    [Fact]
    public async Task Single_generic_SerializingCacheProvider_should_serialize_on_put()
    {
        bool serializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
            deserialize: s => s?.Original ?? default);
        StubCacheProvider stubCacheProvider = new StubCacheProvider();
        object objectToCache = new();
        string key = "some key";

        AsyncSerializingCacheProvider<StubSerialized> serializingCacheProvider = new AsyncSerializingCacheProvider<StubSerialized>(stubCacheProvider.AsyncFor<StubSerialized>(), stubSerializer);
        await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken, false);

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = await stubCacheProvider.TryGetAsync(key, CancellationToken, false);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public async Task Single_generic_SerializingCacheProvider_should_serialize_on_put_for_defaultTResult()
    {
        bool serializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
            deserialize: s => s?.Original ?? default);
        StubCacheProvider stubCacheProvider = new StubCacheProvider();
        object? objectToCache = null;
        string key = "some key";

        AsyncSerializingCacheProvider<StubSerialized> serializingCacheProvider = new AsyncSerializingCacheProvider<StubSerialized>(stubCacheProvider.AsyncFor<StubSerialized>(), stubSerializer);
        await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken, false);

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public async Task Single_generic_SerializingCacheProvider_should_deserialize_on_get()
    {
        bool deserializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => new StubSerialized(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });

        var stubCacheProvider = new StubCacheProvider();
        object objectToCache = new();
        string key = "some key";

        await stubCacheProvider.PutAsync(key, new StubSerialized(objectToCache), new Ttl(TimeSpan.FromMinutes(1)), CancellationToken, false);

        AsyncSerializingCacheProvider<StubSerialized> serializingCacheProvider = new AsyncSerializingCacheProvider<StubSerialized>(stubCacheProvider.AsyncFor<StubSerialized>(), stubSerializer);
        (bool cacheHit, object? fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken, false);

        cacheHit.ShouldBeTrue();
        deserializeInvoked.ShouldBeTrue();
        fromCache.ShouldBe(objectToCache);
    }

    [Fact]
    public async Task Single_generic_SerializingCacheProvider_should_not_deserialize_on_get_when_item_not_in_cache()
    {
        bool deserializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => new StubSerialized(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });
        var stubCacheProvider = new StubCacheProvider();
        string key = "some key";

        stubCacheProvider.TryGet(key).Item1.ShouldBeFalse();

        AsyncSerializingCacheProvider<StubSerialized> serializingCacheProvider = new AsyncSerializingCacheProvider<StubSerialized>(stubCacheProvider.AsyncFor<StubSerialized>(), stubSerializer);
        (bool cacheHit, object? fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken, false);

        cacheHit.ShouldBeFalse();
        deserializeInvoked.ShouldBeFalse();
        fromCache.ShouldBe(default);
    }

    [Fact]
    public async Task Single_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put()
    {
        bool serializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
            deserialize: s => s?.Original ?? default);
        StubCacheProvider stubCacheProvider = new StubCacheProvider();
        object objectToCache = new();
        string key = "some key";

        AsyncSerializingCacheProvider<StubSerialized> serializingCacheProvider = stubCacheProvider.AsyncFor<StubSerialized>().WithSerializer(stubSerializer);
        await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken, false);

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = await stubCacheProvider.TryGetAsync(key, CancellationToken, false);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public async Task Single_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put_for_defaultTResult()
    {
        bool serializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
            deserialize: s => s?.Original ?? default);
        StubCacheProvider stubCacheProvider = new StubCacheProvider();
        object? objectToCache = null;
        string key = "some key";

        AsyncSerializingCacheProvider<StubSerialized> serializingCacheProvider = stubCacheProvider.AsyncFor<StubSerialized>().WithSerializer(stubSerializer);
        await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken, false);

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public async Task Single_generic_SerializingCacheProvider_from_extension_syntax_should_deserialize_on_get()
    {
        var cancellationToken = CancellationToken;
        bool deserializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => new StubSerialized(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });
        var stubCacheProvider = new StubCacheProvider();
        object objectToCache = new();
        string key = "some key";

        await stubCacheProvider.PutAsync(key, new StubSerialized(objectToCache), new Ttl(TimeSpan.FromMinutes(1)), cancellationToken, false);

        AsyncSerializingCacheProvider<StubSerialized> serializingCacheProvider = stubCacheProvider.AsyncFor<StubSerialized>().WithSerializer(stubSerializer);
        (bool cacheHit, object? fromCache) = await serializingCacheProvider.TryGetAsync(key, cancellationToken, false);

        cacheHit.ShouldBeTrue();
        deserializeInvoked.ShouldBeTrue();
        fromCache.ShouldBe(objectToCache);
    }

    [Fact]
    public async Task Single_generic_SerializingCacheProvider_from_extension_syntax_should_not_deserialize_on_get_when_item_not_in_cache()
    {
        bool deserializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => new StubSerialized(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });
        var stubCacheProvider = new StubCacheProvider();
        string key = "some key";

        stubCacheProvider.TryGet(key).Item1.ShouldBeFalse();

        AsyncSerializingCacheProvider<StubSerialized> serializingCacheProvider = stubCacheProvider.AsyncFor<StubSerialized>().WithSerializer(stubSerializer);
        (bool cacheHit, object? fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken, false);

        cacheHit.ShouldBeFalse();
        deserializeInvoked.ShouldBeFalse();
        fromCache.ShouldBe(default);
    }

    #endregion

    #region TResult-to-TSerialized serializer

    [Fact]
    public void Double_generic_constructor_should_throw_on_no_wrapped_cache_provider()
    {
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => new StubSerialized<ResultPrimitive>(o),
            deserialize: s => s?.Original ?? default);

        Action configure = () => _ = new AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(null!, stubTResultSerializer);

        Should.Throw<ArgumentNullException>(configure)
            .ParamName.ShouldBe("wrappedCacheProvider");
    }

    [Fact]
    public void Double_generic_constructor_should_throw_on_no_serializer()
    {
        Action configure = () => _ = new AsyncSerializingCacheProvider<object, object>(new StubCacheProvider().AsyncFor<object>(), null!);

        Should.Throw<ArgumentNullException>(configure)
            .ParamName.ShouldBe("serializer");
    }

    [Fact]
    public void Double_generic_extension_syntax_should_throw_on_no_serializer()
    {
        Action configure = () => new StubCacheProvider().AsyncFor<object>().WithSerializer<object, object>(null!);

        Should.Throw<ArgumentNullException>(configure)
            .ParamName.ShouldBe("serializer");
    }

    [Fact]
    public async Task Double_generic_SerializingCacheProvider_should_serialize_on_put()
    {
        bool serializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
            deserialize: s => s?.Original ?? default);
        var stubCacheProvider = new StubCacheProvider();
        ResultPrimitive objectToCache = ResultPrimitive.Good;
        string key = "some key";

        AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider = new AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);
        await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken, false);

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = await stubCacheProvider.TryGetAsync(key, CancellationToken, false);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized<ResultPrimitive>>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public async Task Double_generic_SerializingCacheProvider_should_serialize_on_put_for_defaultTResult()
    {
        bool serializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
            deserialize: s => s?.Original ?? default);
        StubCacheProvider stubCacheProvider = new StubCacheProvider();
        ResultPrimitive objectToCache = default;
        string key = "some key";

        AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider = new AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);
        await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken, false);

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized<ResultPrimitive>>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public async Task Double_generic_SerializingCacheProvider_should_deserialize_on_get()
    {
        bool deserializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => new StubSerialized<ResultPrimitive>(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });
        var stubCacheProvider = new StubCacheProvider();
        ResultPrimitive objectToCache = ResultPrimitive.Good;
        string key = "some key";

        AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider = new AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);

        await stubCacheProvider.PutAsync(key, new StubSerialized<ResultPrimitive>(objectToCache), new Ttl(TimeSpan.FromMinutes(1)), CancellationToken, false);
        (bool cacheHit, object? fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken, false);

        cacheHit.ShouldBeTrue();
        deserializeInvoked.ShouldBeTrue();
        fromCache.ShouldBe(objectToCache);
    }

    [Fact]
    public async Task Double_generic_SerializingCacheProvider_should_not_deserialize_on_get_when_item_not_in_cache()
    {
        bool deserializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => new StubSerialized<ResultPrimitive>(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });
        var stubCacheProvider = new StubCacheProvider();
        string key = "some key";

        stubCacheProvider.TryGet(key).Item1.ShouldBeFalse();

        AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider = new AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);
        (bool cacheHit, ResultPrimitive? fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken, false);

        cacheHit.ShouldBeFalse();
        deserializeInvoked.ShouldBeFalse();
        fromCache.ShouldBe(ResultPrimitive.Undefined);
    }

    [Fact]
    public async Task Double_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put()
    {
        bool serializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
            deserialize: s => s?.Original ?? default);
        var stubCacheProvider = new StubCacheProvider();
        ResultPrimitive objectToCache = ResultPrimitive.Good;
        string key = "some key";

        AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider =
            stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);
        await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken, false);

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = await stubCacheProvider.TryGetAsync(key, CancellationToken, false);
        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized<ResultPrimitive>>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public async Task Double_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put_for_defaultTResult()
    {
        bool serializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
            deserialize: s => s?.Original ?? default);
        StubCacheProvider stubCacheProvider = new StubCacheProvider();
        ResultPrimitive objectToCache = default;
        string key = "some key";

        AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider =
            stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);
        await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken, false);

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized<ResultPrimitive>>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public async Task Double_generic_SerializingCacheProvider_from_extension_syntax_should_deserialize_on_get()
    {
        bool deserializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => new StubSerialized<ResultPrimitive>(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });
        var stubCacheProvider = new StubCacheProvider();
        ResultPrimitive objectToCache = ResultPrimitive.Good;
        string key = "some key";

        AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider =
            stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);

        await stubCacheProvider.PutAsync(key, new StubSerialized<ResultPrimitive>(objectToCache), new Ttl(TimeSpan.FromMinutes(1)), CancellationToken, false);
        (bool cacheHit, ResultPrimitive? fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken, false);

        cacheHit.ShouldBeTrue();
        deserializeInvoked.ShouldBeTrue();
        fromCache.ShouldBe(objectToCache);
    }

    [Fact]
    public async Task Double_generic_SerializingCacheProvider_from_extension_syntax_should_not_deserialize_on_get_when_item_not_in_cache()
    {
        bool deserializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => new StubSerialized<ResultPrimitive>(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });
        var stubCacheProvider = new StubCacheProvider();
        string key = "some key";

        stubCacheProvider.TryGet(key).Item1.ShouldBeFalse();

        AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider =
            stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);
        (bool cacheHit, ResultPrimitive? fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken, false);

        cacheHit.ShouldBeFalse();
        deserializeInvoked.ShouldBeFalse();
        fromCache.ShouldBe(ResultPrimitive.Undefined);
    }

    #endregion
}
