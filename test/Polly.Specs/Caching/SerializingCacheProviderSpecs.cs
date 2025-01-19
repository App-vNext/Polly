namespace Polly.Specs.Caching;

public class SerializingCacheProviderSpecs
{
    #region Object-to-TSerialized serializer

    [Fact]
    public void Single_generic_constructor_should_throw_on_no_wrapped_cache_provider()
    {
        StubSerializer<object, StubSerialized> stubObjectSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => new StubSerialized(o),
            deserialize: s => s?.Original ?? default);

        Action configure = () => _ = new SerializingCacheProvider<StubSerialized>(null!, stubObjectSerializer);

        Should.Throw<ArgumentNullException>(configure)
            .ParamName.ShouldBe("wrappedCacheProvider");
    }

    [Fact]
    public void Single_generic_constructor_should_throw_on_no_serializer()
    {
        Action configure = () => _ = new SerializingCacheProvider<object>(new StubCacheProvider().For<object>(), null!);

        Should.Throw<ArgumentNullException>(configure)
            .ParamName.ShouldBe("serializer");
    }

    [Fact]
    public void Single_generic_extension_syntax_should_throw_on_no_serializer()
    {
        Action configure = () => new StubCacheProvider().For<object>().WithSerializer(null!);

        Should.Throw<ArgumentNullException>(configure)
            .ParamName.ShouldBe("serializer");
    }

    [Fact]
    public void Single_generic_SerializingCacheProvider_should_serialize_on_put()
    {
        bool serializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
            deserialize: s => s?.Original ?? default);
        StubCacheProvider stubCacheProvider = new StubCacheProvider();
        object objectToCache = new();
        string key = "some key";

        SerializingCacheProvider<StubSerialized> serializingCacheProvider = new SerializingCacheProvider<StubSerialized>(stubCacheProvider.For<StubSerialized>(), stubSerializer);
        serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public void Single_generic_SerializingCacheProvider_should_serialize_on_put_for_defaultTResult()
    {
        bool serializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
            deserialize: s => s?.Original ?? default);
        StubCacheProvider stubCacheProvider = new StubCacheProvider();
        object? objectToCache = null;
        string key = "some key";

        SerializingCacheProvider<StubSerialized> serializingCacheProvider = new SerializingCacheProvider<StubSerialized>(stubCacheProvider.For<StubSerialized>(), stubSerializer);
        serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public void Single_generic_SerializingCacheProvider_should_deserialize_on_get()
    {
        bool deserializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => new StubSerialized(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });

        var stubCacheProvider = new StubCacheProvider();
        object objectToCache = new();
        string key = "some key";

        stubCacheProvider.Put(key, new StubSerialized(objectToCache), new Ttl(TimeSpan.FromMinutes(1)));

        SerializingCacheProvider<StubSerialized> serializingCacheProvider = new SerializingCacheProvider<StubSerialized>(stubCacheProvider.For<StubSerialized>(), stubSerializer);
        (bool cacheHit, object? fromCache) = serializingCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        deserializeInvoked.ShouldBeTrue();
        fromCache.ShouldBe(objectToCache);
    }

    [Fact]
    public void Single_generic_SerializingCacheProvider_should_not_deserialize_on_get_when_item_not_in_cache()
    {
        bool deserializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => new StubSerialized(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });
        var stubCacheProvider = new StubCacheProvider();
        string key = "some key";

        stubCacheProvider.TryGet(key).Item1.ShouldBeFalse();

        SerializingCacheProvider<StubSerialized> serializingCacheProvider = new SerializingCacheProvider<StubSerialized>(stubCacheProvider.For<StubSerialized>(), stubSerializer);
        (bool cacheHit, object? fromCache) = serializingCacheProvider.TryGet(key);

        cacheHit.ShouldBeFalse();
        deserializeInvoked.ShouldBeFalse();
        fromCache.ShouldBe(default);
    }

    [Fact]
    public void Single_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put()
    {
        bool serializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
            deserialize: s => s?.Original);
        StubCacheProvider stubCacheProvider = new StubCacheProvider();
        object objectToCache = new();
        string key = "some key";

        SerializingCacheProvider<StubSerialized> serializingCacheProvider = stubCacheProvider.For<StubSerialized>().WithSerializer(stubSerializer);
        serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public void Single_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put_for_defaultTResult()
    {
        bool serializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
            deserialize: s => s?.Original ?? default);
        StubCacheProvider stubCacheProvider = new StubCacheProvider();
        object? objectToCache = null;
        string key = "some key";

        SerializingCacheProvider<StubSerialized> serializingCacheProvider = stubCacheProvider.For<StubSerialized>().WithSerializer(stubSerializer);
        serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public void Single_generic_SerializingCacheProvider_from_extension_syntax_should_deserialize_on_get()
    {
        bool deserializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => new StubSerialized(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });
        var stubCacheProvider = new StubCacheProvider();
        object objectToCache = new();
        string key = "some key";

        stubCacheProvider.Put(key, new StubSerialized(objectToCache), new Ttl(TimeSpan.FromMinutes(1)));

        SerializingCacheProvider<StubSerialized> serializingCacheProvider = stubCacheProvider.For<StubSerialized>().WithSerializer(stubSerializer);
        (bool cacheHit, object? fromCache) = serializingCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        deserializeInvoked.ShouldBeTrue();
        fromCache.ShouldBe(objectToCache);
    }

    [Fact]
    public void Single_generic_SerializingCacheProvider_from_extension_syntax_should_not_deserialize_on_get_when_item_not_in_cache()
    {
        bool deserializeInvoked = false;
        StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
            serialize: o => new StubSerialized(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });
        var stubCacheProvider = new StubCacheProvider();
        string key = "some key";

        stubCacheProvider.TryGet(key).Item1.ShouldBeFalse();

        SerializingCacheProvider<StubSerialized> serializingCacheProvider = stubCacheProvider.For<StubSerialized>().WithSerializer(stubSerializer);
        (bool cacheHit, object? fromCache) = serializingCacheProvider.TryGet(key);

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

        Action configure = () => _ = new SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(null!, stubTResultSerializer);

        Should.Throw<ArgumentNullException>(configure)
            .ParamName.ShouldBe("wrappedCacheProvider");
    }

    [Fact]
    public void Double_generic_constructor_should_throw_on_no_serializer()
    {
        Action configure = () => _ = new SerializingCacheProvider<object, object>(new StubCacheProvider().For<object>(), null!);

        Should.Throw<ArgumentNullException>(configure)
            .ParamName.ShouldBe("serializer");
    }

    [Fact]
    public void Double_generic_extension_syntax_should_throw_on_no_serializer()
    {
        Action configure = () => new StubCacheProvider().For<object>().WithSerializer<object, object>(null!);

        Should.Throw<ArgumentNullException>(configure)
            .ParamName.ShouldBe("serializer");
    }

    [Fact]
    public void Double_generic_SerializingCacheProvider_should_serialize_on_put()
    {
        bool serializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
            deserialize: s => s?.Original ?? default);
        var stubCacheProvider = new StubCacheProvider();
        ResultPrimitive objectToCache = ResultPrimitive.Good;
        string key = "some key";

        SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider = new SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.For<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);
        serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized<ResultPrimitive>>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public void Double_generic_SerializingCacheProvider_should_serialize_on_put_for_defaultTResult()
    {
        bool serializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
            deserialize: s => s?.Original ?? default);
        StubCacheProvider stubCacheProvider = new StubCacheProvider();
        ResultPrimitive objectToCache = default;
        string key = "some key";

        SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider = new SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.For<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);
        serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized<ResultPrimitive>>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public void Double_generic_SerializingCacheProvider_should_deserialize_on_get()
    {
        bool deserializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => new StubSerialized<ResultPrimitive>(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });
        var stubCacheProvider = new StubCacheProvider();
        ResultPrimitive objectToCache = ResultPrimitive.Good;
        string key = "some key";

        SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider = new SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.For<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);

        stubCacheProvider.Put(key, new StubSerialized<ResultPrimitive>(objectToCache), new Ttl(TimeSpan.FromMinutes(1)));
        (bool cacheHit, object fromCache) = serializingCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        deserializeInvoked.ShouldBeTrue();
        fromCache.ShouldBe(objectToCache);
    }

    [Fact]
    public void Double_generic_SerializingCacheProvider_should_not_deserialize_on_get_when_item_not_in_cache()
    {
        bool deserializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => new StubSerialized<ResultPrimitive>(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });
        var stubCacheProvider = new StubCacheProvider();
        string key = "some key";

        stubCacheProvider.TryGet(key).Item1.ShouldBeFalse();

        SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider = new SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.For<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);
        (bool cacheHit, ResultPrimitive fromCache) = serializingCacheProvider.TryGet(key);

        cacheHit.ShouldBeFalse();
        deserializeInvoked.ShouldBeFalse();
        fromCache.ShouldBe(default);
    }

    [Fact]
    public void Double_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put()
    {
        bool serializeInvoked = false;
        var stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
            deserialize: s => s?.Original ?? default);
        var stubCacheProvider = new StubCacheProvider();
        ResultPrimitive objectToCache = ResultPrimitive.Good;
        string key = "some key";

        SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider =
            stubCacheProvider.For<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);
        serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized<ResultPrimitive>>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public void Double_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put_for_defaultTResult()
    {
        bool serializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
            deserialize: s => s?.Original ?? default);
        StubCacheProvider stubCacheProvider = new StubCacheProvider();
        ResultPrimitive objectToCache = default;
        string key = "some key";

        SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider =
            stubCacheProvider.For<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);

        serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

        serializeInvoked.ShouldBeTrue();

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        fromCache.ShouldBeOfType<StubSerialized<ResultPrimitive>>()
            .Original.ShouldBe(objectToCache);
    }

    [Fact]
    public void Double_generic_SerializingCacheProvider_from_extension_syntax_should_deserialize_on_get()
    {
        bool deserializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => new StubSerialized<ResultPrimitive>(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });
        var stubCacheProvider = new StubCacheProvider();
        ResultPrimitive objectToCache = ResultPrimitive.Good;
        string key = "some key";

        SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider =
            stubCacheProvider.For<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);

        stubCacheProvider.Put(key, new StubSerialized<ResultPrimitive>(objectToCache), new Ttl(TimeSpan.FromMinutes(1)));
        (bool cacheHit, ResultPrimitive? fromCache) = serializingCacheProvider.TryGet(key);

        cacheHit.ShouldBeTrue();
        deserializeInvoked.ShouldBeTrue();
        fromCache.ShouldBe(objectToCache);
    }

    [Fact]
    public void Double_generic_SerializingCacheProvider_from_extension_syntax_should_not_deserialize_on_get_when_item_not_in_cache()
    {
        bool deserializeInvoked = false;
        StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
            serialize: o => new StubSerialized<ResultPrimitive>(o),
            deserialize: s => { deserializeInvoked = true; return s?.Original ?? default; });
        var stubCacheProvider = new StubCacheProvider();
        string key = "some key";

        stubCacheProvider.TryGet(key).Item2.ShouldBeNull();

        SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider =
            stubCacheProvider.For<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);
        (bool cacheHit, ResultPrimitive? fromCache) = serializingCacheProvider.TryGet(key);

        cacheHit.ShouldBeFalse();
        deserializeInvoked.ShouldBeFalse();
        fromCache.ShouldBe(ResultPrimitive.Undefined);
    }

    #endregion
}
