using System;
using FluentAssertions;
using Polly.Caching;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs.Caching
{
    public class SerializingCacheProviderSpecs
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

        #region Configuration

        [Fact]
        public void Single_generic_constructor_should_throw_on_no_wrapped_cache_provider()
        {
            var mockSerializer = new ManualMockSerializer(o => o, o => o);

            Action configure = () => new SerializingCacheProvider<object>(null, mockSerializer);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("wrappedCacheProvider");
        }

        [Fact]
        public void Single_generic_constructor_should_throw_on_no_serializer()
        {
            Action configure = () => new SerializingCacheProvider<object>(new StubCacheProvider().As<object>(), null);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public void Single_generic_extension_syntax_should_throw_on_no_serializer()
        {
            Action configure = () => new StubCacheProvider().As<object>().WithSerializer(null);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        #endregion

        [Fact]
        public void Single_generic_SerializingCacheProvider_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            ManualMockSerializer mockSerializer = new ManualMockSerializer(o => { serializeInvoked = true; return new Tuple<object>(o); }, o => o);
            StubCacheProvider stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProvider<object> serializingCacheProvider = new SerializingCacheProvider<object>(stubCacheProvider.As<object>(), mockSerializer);
            serializingCacheProvider.Put(key, objectToCache, TimeSpan.FromMinutes(1));

            serializeInvoked.Should().Be(true);
            stubCacheProvider.Get(key).Should().BeOfType<Tuple<object>>()
                .Which.Item1.Should().Be(objectToCache);
        }

        [Fact]
        public void Single_generic_SerializingCacheProvider_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            var mockSerializer = new ManualMockSerializer(o => o, o => { deserializeInvoked = true; return ((Tuple<object>)o).Item1; });
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            stubCacheProvider.Put(key, new Tuple<object>(objectToCache), TimeSpan.FromMinutes(1));

            SerializingCacheProvider<object> serializingCacheProvider = new SerializingCacheProvider<object>(stubCacheProvider.As<object>(), mockSerializer);
            object fromCache = serializingCacheProvider.Get(key);

            deserializeInvoked.Should().Be(true);
            fromCache.Should().Be(objectToCache);
        }

        [Fact]
        public void Single_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            ManualMockSerializer mockSerializer = new ManualMockSerializer(o => { serializeInvoked = true; return new Tuple<object>(o); }, o => o);
            StubCacheProvider stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProvider<object> serializingCacheProvider = stubCacheProvider.As<object>().WithSerializer(mockSerializer);
            serializingCacheProvider.Put(key, objectToCache, TimeSpan.FromMinutes(1));

            serializeInvoked.Should().Be(true);
            stubCacheProvider.Get(key).Should().BeOfType<Tuple<object>>()
                .Which.Item1.Should().Be(objectToCache);
        }

        [Fact]
        public void Single_generic_SerializingCacheProvider_from_extension_syntax_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            var mockSerializer = new ManualMockSerializer(o => o, o => { deserializeInvoked = true; return ((Tuple<object>)o).Item1; });
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            stubCacheProvider.Put(key, new Tuple<object>(objectToCache), TimeSpan.FromMinutes(1));

            SerializingCacheProvider<object> serializingCacheProvider = stubCacheProvider.As<object>().WithSerializer(mockSerializer);
            object fromCache = serializingCacheProvider.Get(key);

            deserializeInvoked.Should().Be(true);
            fromCache.Should().Be(objectToCache);
        }

        #endregion

        #region TResult-to-TSerialized serializer

        [Fact]
        public void Double_generic_constructor_should_throw_on_no_wrapped_cache_provider()
        {
            var mockSerializer = new ManualMockSerializer(o => o, o => o);

            Action configure = () => new SerializingCacheProvider<object, object>(null, mockSerializer);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("wrappedCacheProvider");
        }

        [Fact]
        public void Double_generic_constructor_should_throw_on_no_serializer()
        {
            Action configure = () => new SerializingCacheProvider<object, object>(new StubCacheProvider().As<object>(), null);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public void Double_generic_extension_syntax_should_throw_on_no_serializer()
        {
            Action configure = () => new StubCacheProvider().As<object>().WithSerializer<object, object>(null);

            configure.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public void Double_generic_SerializingCacheProvider_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            var mockSerializer = new ManualMockSerializer(o => { serializeInvoked = true; return new Tuple<object>(o); }, o => o);
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProvider<object, object> serializingCacheProvider = new SerializingCacheProvider<object, object>(stubCacheProvider.As<object>(), mockSerializer);
            serializingCacheProvider.Put(key, objectToCache, TimeSpan.FromMinutes(1));

            serializeInvoked.Should().Be(true);
            stubCacheProvider.Get(key).Should().BeOfType<Tuple<object>>()
                .Which.Item1.Should().Be(objectToCache);
        }

        [Fact]
        public void Double_generic_SerializingCacheProvider_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            var mockSerializer = new ManualMockSerializer(o => o, o => { deserializeInvoked = true; return ((Tuple<object>)o).Item1; });
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProvider<object, object> serializingCacheProvider = new SerializingCacheProvider<object, object>(stubCacheProvider.As<object>(), mockSerializer);

            stubCacheProvider.Put(key, new Tuple<object>(objectToCache), TimeSpan.FromMinutes(1));
            object fromCache = serializingCacheProvider.Get(key);

            deserializeInvoked.Should().Be(true);
            fromCache.Should().Be(objectToCache);
        }

        [Fact]
        public void Double_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            var mockSerializer = new ManualMockSerializer(o => { serializeInvoked = true; return new Tuple<object>(o); }, o => o);
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProvider<object, object> serializingCacheProvider = stubCacheProvider.As<object>().WithSerializer<object, object>(mockSerializer);
            serializingCacheProvider.Put(key, objectToCache, TimeSpan.FromMinutes(1));

            serializeInvoked.Should().Be(true);
            stubCacheProvider.Get(key).Should().BeOfType<Tuple<object>>()
                .Which.Item1.Should().Be(objectToCache);
        }

        [Fact]
        public void Double_generic_SerializingCacheProvider_from_extension_syntax_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            var mockSerializer = new ManualMockSerializer(o => o, o => { deserializeInvoked = true; return ((Tuple<object>)o).Item1; });
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProvider<object, object> serializingCacheProvider = stubCacheProvider.As<object>().WithSerializer<object, object>(mockSerializer);

            stubCacheProvider.Put(key, new Tuple<object>(objectToCache), TimeSpan.FromMinutes(1));
            object fromCache = serializingCacheProvider.Get(key);

            deserializeInvoked.Should().Be(true);
            fromCache.Should().Be(objectToCache);
        }

        #endregion
    }
}
