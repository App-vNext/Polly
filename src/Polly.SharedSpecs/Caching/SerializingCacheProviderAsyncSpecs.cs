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
    public class SerializingCacheProviderAsyncSpecs
    {
        #region Object-to-TSerialized serializer 

        [Fact]
        public void Single_generic_constructor_should_throw_on_no_wrapped_cache_provider()
        {
            StubSerializer<object, StubSerialized> stubObjectSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => new StubSerialized(o),
                deserialize: s => s.Original
            );

            Action configure = () => new SerializingCacheProviderAsync<StubSerialized>(null, stubObjectSerializer);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("wrappedCacheProvider");
        }

        [Fact]
        public void Single_generic_constructor_should_throw_on_no_serializer()
        {
            Action configure = () => new SerializingCacheProviderAsync<object>(new StubCacheProvider().AsyncFor<object>(), null);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public void Single_generic_extension_syntax_should_throw_on_no_serializer()
        {
            Action configure = () => new StubCacheProvider().AsyncFor<object>().WithSerializer(null);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public async Task Single_generic_SerializingCacheProvider_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
                deserialize: s => s.Original
            );
            StubCacheProvider stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProviderAsync<StubSerialized> serializingCacheProvider = new SerializingCacheProviderAsync<StubSerialized>(stubCacheProvider.AsyncFor<StubSerialized>(), stubSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            serializeInvoked.Should().Be(true);
            (await stubCacheProvider.GetAsync(key, CancellationToken.None, false)).Should().BeOfType<StubSerialized>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Single_generic_SerializingCacheProvider_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => new StubSerialized(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );

            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            await stubCacheProvider.PutAsync(key, new StubSerialized(objectToCache), new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            SerializingCacheProviderAsync<StubSerialized> serializingCacheProvider = new SerializingCacheProviderAsync<StubSerialized>(stubCacheProvider.AsyncFor<StubSerialized>(), stubSerializer);
            object fromCache = await serializingCacheProvider.GetAsync(key, CancellationToken.None, false);

            deserializeInvoked.Should().Be(true);
            fromCache.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Single_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
                deserialize: s => s.Original
            );
            StubCacheProvider stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProviderAsync<StubSerialized> serializingCacheProvider = stubCacheProvider.AsyncFor<StubSerialized>().WithSerializer(stubSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            serializeInvoked.Should().Be(true);
            (await stubCacheProvider.GetAsync(key, CancellationToken.None, false)).Should().BeOfType<StubSerialized>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Single_generic_SerializingCacheProvider_from_extension_syntax_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => new StubSerialized(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            await stubCacheProvider.PutAsync(key, new StubSerialized(objectToCache), new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            SerializingCacheProviderAsync<StubSerialized> serializingCacheProvider = stubCacheProvider.AsyncFor<StubSerialized>().WithSerializer(stubSerializer);
            object fromCache = await serializingCacheProvider.GetAsync(key, CancellationToken.None, false);

            deserializeInvoked.Should().Be(true);
            fromCache.Should().Be(objectToCache);
        }

        #endregion

        #region TResult-to-TSerialized serializer

        [Fact]
        public void Double_generic_constructor_should_throw_on_no_wrapped_cache_provider()
        {
            StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => new StubSerialized<ResultPrimitive>(o),
                deserialize: s => s.Original
            );

            Action configure = () => new SerializingCacheProviderAsync<ResultPrimitive, StubSerialized<ResultPrimitive>>(null, stubTResultSerializer);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("wrappedCacheProvider");
        }

        [Fact]
        public void Double_generic_constructor_should_throw_on_no_serializer()
        {
            Action configure = () => new SerializingCacheProviderAsync<object, object>(new StubCacheProvider().AsyncFor<object>(), null);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public void Double_generic_extension_syntax_should_throw_on_no_serializer()
        {
            Action configure = () => new StubCacheProvider().AsyncFor<object>().WithSerializer<object, object>(null);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public async Task Double_generic_SerializingCacheProvider_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
                deserialize: s => s.Original
            );
            var stubCacheProvider = new StubCacheProvider();
            ResultPrimitive objectToCache = ResultPrimitive.Good;
            string key = "some key";

            SerializingCacheProviderAsync<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider = new SerializingCacheProviderAsync<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            serializeInvoked.Should().Be(true);
            (await stubCacheProvider.GetAsync(key, CancellationToken.None, false)).Should().BeOfType<StubSerialized<ResultPrimitive>>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Double_generic_SerializingCacheProvider_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => new StubSerialized<ResultPrimitive>(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            ResultPrimitive objectToCache = ResultPrimitive.Good;
            string key = "some key";

            SerializingCacheProviderAsync<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider = new SerializingCacheProviderAsync<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);

            await stubCacheProvider.PutAsync(key, new StubSerialized<ResultPrimitive>(objectToCache), new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);
            object fromCache = await serializingCacheProvider.GetAsync(key, CancellationToken.None, false);

            deserializeInvoked.Should().Be(true);
            fromCache.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Double_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
                deserialize: s => s.Original
            );
            var stubCacheProvider = new StubCacheProvider();
            ResultPrimitive objectToCache = ResultPrimitive.Good;
            string key = "some key";

            SerializingCacheProviderAsync<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider =
                stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);

            serializeInvoked.Should().Be(true);
            (await stubCacheProvider.GetAsync(key, CancellationToken.None, false)).Should().BeOfType<StubSerialized<ResultPrimitive>>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public async Task Double_generic_SerializingCacheProvider_from_extension_syntax_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => new StubSerialized<ResultPrimitive>(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            ResultPrimitive objectToCache = ResultPrimitive.Good;
            string key = "some key";

            SerializingCacheProviderAsync<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider =
                stubCacheProvider.AsyncFor<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);

            await stubCacheProvider.PutAsync(key, new StubSerialized<ResultPrimitive>(objectToCache), new Ttl(TimeSpan.FromMinutes(1)), CancellationToken.None, false);
            object fromCache = await serializingCacheProvider.GetAsync(key, CancellationToken.None, false);

            deserializeInvoked.Should().Be(true);
            fromCache.Should().Be(objectToCache);
        }

        #endregion
    }
}