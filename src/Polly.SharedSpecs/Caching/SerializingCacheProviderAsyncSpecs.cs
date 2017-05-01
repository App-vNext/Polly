using System;
using System.Threading;
using FluentAssertions;
using Polly.Caching;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs.Caching
{
    public class SerializingCacheProviderAsyncSpecs
    {

        #region Helper classes

        private class ManualMockSerializer : ICacheItemSerializer<object, object>
        {
            private readonly Func<object, object> _onSerialize;
            private readonly Func<object, object> _onDeserialize;

            public ManualMockSerializer(Func<object, object> onSerialize, Func<object, object> onDeserialize)
            {
                _onSerialize = onSerialize;
                _onDeserialize = onDeserialize;
            }
            public object Serialize(object objectToSerialize) => _onSerialize(objectToSerialize);

            public object Deserialize(object objectToDeserialize) => _onDeserialize(objectToDeserialize);
        }

        #endregion

        #region Object-to-TSerialized serializer 

        [Fact]
        public void Single_generic_constructor_should_throw_on_no_wrapped_cache_provider()
        {
            var mockSerializer = new ManualMockSerializer(o => o, o => o);

            Action configure = () => new SerializingCacheProviderAsync<object>(null, mockSerializer);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("wrappedCacheProvider");
        }

        [Fact]
        public void Single_generic_constructor_should_throw_on_no_serializer()
        {
            Action configure = () => new SerializingCacheProviderAsync<object>(new StubCacheProvider().AsyncAs<object>(), null);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public void Single_generic_extension_syntax_should_throw_on_no_serializer()
        {
            Action configure = () => new StubCacheProvider().AsyncAs<object>().WithSerializer(null);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public async void Single_generic_SerializingCacheProvider_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            ManualMockSerializer mockSerializer = new ManualMockSerializer(o => { serializeInvoked = true; return new Tuple<object>(o); }, o => o);
            StubCacheProvider stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProviderAsync<object> serializingCacheProvider = new SerializingCacheProviderAsync<object>(stubCacheProvider.AsyncAs<object>(), mockSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, TimeSpan.FromMinutes(1), CancellationToken.None, false);

            serializeInvoked.Should().Be(true);
            stubCacheProvider.Get(key).Should().BeOfType<Tuple<object>>()
                .Which.Item1.Should().Be(objectToCache);
        }

        [Fact]
        public async void Single_generic_SerializingCacheProvider_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            var mockSerializer = new ManualMockSerializer(o => o, o => { deserializeInvoked = true; return ((Tuple<object>)o).Item1; });
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            stubCacheProvider.Put(key, new Tuple<object>(objectToCache), TimeSpan.FromMinutes(1));

            SerializingCacheProviderAsync<object> serializingCacheProvider = new SerializingCacheProviderAsync<object>(stubCacheProvider.AsyncAs<object>(), mockSerializer);
            object fromCache = await serializingCacheProvider.GetAsync(key, CancellationToken.None, false);

            deserializeInvoked.Should().Be(true);
            fromCache.Should().Be(objectToCache);
        }

        [Fact]
        public async void Single_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            ManualMockSerializer mockSerializer = new ManualMockSerializer(o => { serializeInvoked = true; return new Tuple<object>(o); }, o => o);
            StubCacheProvider stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProviderAsync<object> serializingCacheProvider = stubCacheProvider.AsyncAs<object>().WithSerializer(mockSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, TimeSpan.FromMinutes(1), CancellationToken.None, false);

            serializeInvoked.Should().Be(true);
            stubCacheProvider.Get(key).Should().BeOfType<Tuple<object>>()
                .Which.Item1.Should().Be(objectToCache);
        }

        [Fact]
        public async void Single_generic_SerializingCacheProvider_from_extension_syntax_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            var mockSerializer = new ManualMockSerializer(o => o, o => { deserializeInvoked = true; return ((Tuple<object>)o).Item1; });
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            stubCacheProvider.Put(key, new Tuple<object>(objectToCache), TimeSpan.FromMinutes(1));

            SerializingCacheProviderAsync<object> serializingCacheProvider = stubCacheProvider.AsyncAs<object>().WithSerializer(mockSerializer);
            object fromCache = await serializingCacheProvider.GetAsync(key, CancellationToken.None, false);

            deserializeInvoked.Should().Be(true);
            fromCache.Should().Be(objectToCache);
        }

        #endregion

        #region TResult-to-TSerialized serializer

        [Fact]
        public void Double_generic_constructor_should_throw_on_no_wrapped_cache_provider()
        {
            var mockSerializer = new ManualMockSerializer(o => o, o => o);

            Action configure = () => new SerializingCacheProviderAsync<object, object>(null, mockSerializer);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("wrappedCacheProvider");
        }

        [Fact]
        public void Double_generic_constructor_should_throw_on_no_serializer()
        {
            Action configure = () => new SerializingCacheProviderAsync<object, object>(new StubCacheProvider().AsyncAs<object>(), null);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public void Double_generic_extension_syntax_should_throw_on_no_serializer()
        {
            Action configure = () => new StubCacheProvider().AsyncAs<object>().WithSerializer<object, object>(null);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public async void Double_generic_SerializingCacheProvider_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            var mockSerializer = new ManualMockSerializer(o => { serializeInvoked = true; return new Tuple<object>(o); }, o => o);
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProviderAsync<object, object> serializingCacheProvider = new SerializingCacheProviderAsync<object, object>(stubCacheProvider.AsyncAs<object>(), mockSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, TimeSpan.FromMinutes(1), CancellationToken.None, false);

            serializeInvoked.Should().Be(true);
            stubCacheProvider.Get(key).Should().BeOfType<Tuple<object>>()
                .Which.Item1.Should().Be(objectToCache);
        }

        [Fact]
        public async void Double_generic_SerializingCacheProvider_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            var mockSerializer = new ManualMockSerializer(o => o, o => { deserializeInvoked = true; return ((Tuple<object>)o).Item1; });
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProviderAsync<object, object> serializingCacheProvider = new SerializingCacheProviderAsync<object, object>(stubCacheProvider.AsyncAs<object>(), mockSerializer);

            stubCacheProvider.Put(key, new Tuple<object>(objectToCache), TimeSpan.FromMinutes(1));
            object fromCache = await serializingCacheProvider.GetAsync(key, CancellationToken.None, false);

            deserializeInvoked.Should().Be(true);
            fromCache.Should().Be(objectToCache);
        }

        [Fact]
        public async void Double_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            var mockSerializer = new ManualMockSerializer(o => { serializeInvoked = true; return new Tuple<object>(o); }, o => o);
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProviderAsync<object, object> serializingCacheProvider = stubCacheProvider.AsyncAs<object>().WithSerializer<object, object>(mockSerializer);
            await serializingCacheProvider.PutAsync(key, objectToCache, TimeSpan.FromMinutes(1), CancellationToken.None, false);

            serializeInvoked.Should().Be(true);
            stubCacheProvider.Get(key).Should().BeOfType<Tuple<object>>()
                .Which.Item1.Should().Be(objectToCache);
        }

        [Fact]
        public async void Double_generic_SerializingCacheProvider_from_extension_syntax_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            var mockSerializer = new ManualMockSerializer(o => o, o => { deserializeInvoked = true; return ((Tuple<object>)o).Item1; });
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProviderAsync<object, object> serializingCacheProvider = stubCacheProvider.AsyncAs<object>().WithSerializer<object, object>(mockSerializer);

            stubCacheProvider.Put(key, new Tuple<object>(objectToCache), TimeSpan.FromMinutes(1));
            object fromCache = await serializingCacheProvider.GetAsync(key, CancellationToken.None, false);

            deserializeInvoked.Should().Be(true);
            fromCache.Should().Be(objectToCache);
        }

        #endregion
    }
}
