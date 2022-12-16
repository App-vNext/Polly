using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Caching;
using Polly.Specs.Helpers;
using Polly.Specs.Helpers.Caching;
using Xunit;

namespace Polly.Specs.Caching
{
    public class AsyncSerializingCacheProviderSpecs
    {
        #region Object-to-TSerialized serializer 

        [Fact]
        public void Single_generic_constructor_should_throw_on_no_wrapped_cache_provider()
        {
            var stubObjectSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => new StubSerialized(o),
                deserialize: s => s.Original
            );

            Action configure = () => new AsyncSerializingCacheProvider<StubSerialized>(null, stubObjectSerializer);

            configure.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("wrappedCacheProvider");
        }

        [Fact]
        public void Single_generic_constructor_should_throw_on_no_serializer()
        {
            Action configure = () => new AsyncSerializingCacheProvider<object>(new StubCacheProvider().AsyncFor<object>(), null);

            configure.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public void Single_generic_extension_syntax_should_throw_on_no_serializer()
        {
            Action configure = () => new StubCacheProvider().AsyncFor<object>().WithSerializer(null);

            configure.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public async Task Single_generic_SerializingCacheProvider_should_serialize_on_put()
        {
            var serializeInvoked = false;
            var stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
                deserialize: s => s.Original
            );
            var stubCacheProvider = new StubCacheProvider();
            var objectToCache = new object();
            var key = "some key";

            var serializingCacheProvider = new AsyncSerializingCacheProvider<StubSerialized>(stubCacheProvider.AsyncFor<StubSerialized>(), stubSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            serializeInvoked.Should().BeTrue();

            (var cacheHit, var fromCache) = await stubCacheProvider.TryGetAsync(key, CancellationToken.None, false);

            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Single_generic_SerializingCacheProvider_should_serialize_on_put_for_defaultTResult()
        {
            var serializeInvoked = false;
            var stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
                deserialize: s => s.Original
            );
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = default;
            var key = "some key";

            var serializingCacheProvider = new AsyncSerializingCacheProvider<StubSerialized>(stubCacheProvider.AsyncFor<StubSerialized>(), stubSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            serializeInvoked.Should().BeTrue();

            (var cacheHit, var fromCache) = stubCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Single_generic_SerializingCacheProvider_should_deserialize_on_get()
        {
            var deserializeInvoked = false;
            var stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => new StubSerialized(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );

            var stubCacheProvider = new StubCacheProvider();
            var objectToCache = new object();
            var key = "some key";

            await stubCacheProvider.PutAsync(key, new StubSerialized(objectToCache), new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            var serializingCacheProvider = new AsyncSerializingCacheProvider<StubSerialized>(stubCacheProvider.AsyncFor<StubSerialized>(), stubSerializer);
            (var cacheHit, var fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken.None, false);

            cacheHit.Should().BeTrue();
            deserializeInvoked.Should().BeTrue();
            fromCache.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Single_generic_SerializingCacheProvider_should_not_deserialize_on_get_when_item_not_in_cache()
        {
            var deserializeInvoked = false;
            var stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => new StubSerialized(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            var key = "some key";

            stubCacheProvider.TryGet(key).Item1.Should().BeFalse();

            var serializingCacheProvider = new AsyncSerializingCacheProvider<StubSerialized>(stubCacheProvider.AsyncFor<StubSerialized>(), stubSerializer);
            (var cacheHit, var fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken.None, false);

            cacheHit.Should().BeFalse();
            deserializeInvoked.Should().BeFalse();
            fromCache.Should().Be(default);
        }

        [Fact]
        public async Task Single_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put()
        {
            var serializeInvoked = false;
            var stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
                deserialize: s => s.Original
            );
            var stubCacheProvider = new StubCacheProvider();
            var objectToCache = new object();
            var key = "some key";

            var serializingCacheProvider = stubCacheProvider.AsyncFor<StubSerialized>().WithSerializer(stubSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            serializeInvoked.Should().BeTrue();

            (var cacheHit, var fromCache) = await stubCacheProvider.TryGetAsync(key, CancellationToken.None, false);

            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Single_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put_for_defaultTResult()
        {
            var serializeInvoked = false;
            var stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
                deserialize: s => s.Original
            );
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = default;
            var key = "some key";

            var serializingCacheProvider = stubCacheProvider.AsyncFor<StubSerialized>().WithSerializer(stubSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            serializeInvoked.Should().BeTrue();

            (var cacheHit, var fromCache) = stubCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Single_generic_SerializingCacheProvider_from_extension_syntax_should_deserialize_on_get()
        {
            var deserializeInvoked = false;
            var stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => new StubSerialized(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            var objectToCache = new object();
            var key = "some key";

            await stubCacheProvider.PutAsync(key, new StubSerialized(objectToCache), new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            var serializingCacheProvider = stubCacheProvider.AsyncFor<StubSerialized>().WithSerializer(stubSerializer);
            (var cacheHit, var fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken.None, false);

            cacheHit.Should().BeTrue();
            deserializeInvoked.Should().BeTrue();
            fromCache.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Single_generic_SerializingCacheProvider_from_extension_syntax_should_not_deserialize_on_get_when_item_not_in_cache()
        {
            var deserializeInvoked = false;
            var stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => new StubSerialized(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            var key = "some key";

            stubCacheProvider.TryGet(key).Item1.Should().BeFalse();

            var serializingCacheProvider = stubCacheProvider.AsyncFor<StubSerialized>().WithSerializer(stubSerializer);
            (var cacheHit, var fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken.None, false);

            cacheHit.Should().BeFalse();
            deserializeInvoked.Should().BeFalse();
            fromCache.Should().Be(default);
        }

        #endregion

        #region TResult-to-TSerialized serializer

        [Fact]
        public void Double_generic_constructor_should_throw_on_no_wrapped_cache_provider()
        {
            var stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => new StubSerialized<ResultPrimitive>(o),
                deserialize: s => s.Original
            );

            Action configure = () => new AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(null, stubTResultSerializer);

            configure.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("wrappedCacheProvider");
        }

        [Fact]
        public void Double_generic_constructor_should_throw_on_no_serializer()
        {
            Action configure = () => new AsyncSerializingCacheProvider<object, object>(new StubCacheProvider().AsyncFor<object>(), null);

            configure.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public void Double_generic_extension_syntax_should_throw_on_no_serializer()
        {
            Action configure = () => new StubCacheProvider().AsyncFor<object>().WithSerializer<object, object>(null);

            configure.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public async Task Double_generic_SerializingCacheProvider_should_serialize_on_put()
        {
            var serializeInvoked = false;
            var stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
                deserialize: s => s.Original
            );
            var stubCacheProvider = new StubCacheProvider();
            var objectToCache = ResultPrimitive.Good;
            var key = "some key";

            var serializingCacheProvider = new AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            serializeInvoked.Should().BeTrue();

            (var cacheHit, var fromCache) = await stubCacheProvider.TryGetAsync(key, CancellationToken.None, false);

            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized<ResultPrimitive>>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Double_generic_SerializingCacheProvider_should_serialize_on_put_for_defaultTResult()
        {
            var serializeInvoked = false;
            var stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
                deserialize: s => s.Original
            );
            var stubCacheProvider = new StubCacheProvider();
            ResultPrimitive objectToCache = default;
            var key = "some key";

            var serializingCacheProvider = new AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            serializeInvoked.Should().BeTrue();

            (var cacheHit, var fromCache) = stubCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized<ResultPrimitive>>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Double_generic_SerializingCacheProvider_should_deserialize_on_get()
        {
            var deserializeInvoked = false;
            var stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => new StubSerialized<ResultPrimitive>(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            var objectToCache = ResultPrimitive.Good;
            var key = "some key";

            var serializingCacheProvider = new AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);

            await stubCacheProvider.PutAsync(key, new StubSerialized<ResultPrimitive>(objectToCache), new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);
            (var cacheHit, object fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken.None, false);

            cacheHit.Should().BeTrue();
            deserializeInvoked.Should().BeTrue();
            fromCache.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Double_generic_SerializingCacheProvider_should_not_deserialize_on_get_when_item_not_in_cache()
        {
            var deserializeInvoked = false;
            var stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => new StubSerialized<ResultPrimitive>(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            var key = "some key";

            stubCacheProvider.TryGet(key).Item1.Should().BeFalse();

            var serializingCacheProvider = new AsyncSerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);
            (var cacheHit, var fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken.None, false);

            cacheHit.Should().BeFalse();
            deserializeInvoked.Should().BeFalse();
            fromCache.Should().Be(default(ResultPrimitive));
        }

        [Fact]
        public async Task Double_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put()
        {
            var serializeInvoked = false;
            var stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
                deserialize: s => s.Original
            );
            var stubCacheProvider = new StubCacheProvider();
            var objectToCache = ResultPrimitive.Good;
            var key = "some key";

            var serializingCacheProvider =
                stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            serializeInvoked.Should().BeTrue();

            (var cacheHit, var fromCache) = await stubCacheProvider.TryGetAsync(key, CancellationToken.None, false);
            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized<ResultPrimitive>>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Double_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put_for_defaultTResult()
        {
            var serializeInvoked = false;
            var stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
                deserialize: s => s.Original
            );
            var stubCacheProvider = new StubCacheProvider();
            ResultPrimitive objectToCache = default;
            var key = "some key";

            var serializingCacheProvider =
                stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            serializeInvoked.Should().BeTrue();

            (var cacheHit, var fromCache) = stubCacheProvider.TryGet(key);
            
            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized<ResultPrimitive>>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Double_generic_SerializingCacheProvider_from_extension_syntax_should_deserialize_on_get()
        {
            var deserializeInvoked = false;
            var stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => new StubSerialized<ResultPrimitive>(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            var objectToCache = ResultPrimitive.Good;
            var key = "some key";

            var serializingCacheProvider =
                stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);

            await stubCacheProvider.PutAsync(key, new StubSerialized<ResultPrimitive>(objectToCache), new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);
            (var cacheHit, var fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken.None, false);

            cacheHit.Should().BeTrue();
            deserializeInvoked.Should().BeTrue();
            fromCache.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Double_generic_SerializingCacheProvider_from_extension_syntax_should_not_deserialize_on_get_when_item_not_in_cache()
        {
            var deserializeInvoked = false;
            var stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => new StubSerialized<ResultPrimitive>(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            var key = "some key";

            stubCacheProvider.TryGet(key).Item1.Should().BeFalse();

            var serializingCacheProvider =
                stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);
            (var cacheHit, var fromCache) = await serializingCacheProvider.TryGetAsync(key, CancellationToken.None, false);

            cacheHit.Should().BeFalse();
            deserializeInvoked.Should().BeFalse();
            fromCache.Should().Be(default(ResultPrimitive));
        }

        #endregion
    }
}