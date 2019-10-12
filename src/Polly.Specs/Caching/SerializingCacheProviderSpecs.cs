﻿using System;
using FluentAssertions;
using Polly.Caching;
using Polly.Specs.Helpers;
using Polly.Specs.Helpers.Caching;
using Xunit;

namespace Polly.Specs.Caching
{
    public class SerializingCacheProviderSpecs
    {
        #region Object-to-TSerialized serializer 

        [Fact]
        public void Single_generic_constructor_should_throw_on_no_wrapped_cache_provider()
        {
            StubSerializer<object, StubSerialized> stubObjectSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => new StubSerialized(o),
                deserialize: s => s.Original
            );

        Action configure = () => new SerializingCacheProvider<StubSerialized>(null, stubObjectSerializer);

            configure.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("wrappedCacheProvider");
        }

        [Fact]
        public void Single_generic_constructor_should_throw_on_no_serializer()
        {
            Action configure = () => new SerializingCacheProvider<object>(new StubCacheProvider().For<object>(), null);

            configure.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public void Single_generic_extension_syntax_should_throw_on_no_serializer()
        {
            Action configure = () => new StubCacheProvider().For<object>().WithSerializer(null);

            configure.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public void Single_generic_SerializingCacheProvider_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => { serializeInvoked = true; return new StubSerialized(o);},
                deserialize: s => s.Original
            );
            StubCacheProvider stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProvider<StubSerialized> serializingCacheProvider = new SerializingCacheProvider<StubSerialized>(stubCacheProvider.For<StubSerialized>(), stubSerializer);
            serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

            serializeInvoked.Should().BeTrue();

            (bool cacheHit, object fromCache) = stubCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public void Single_generic_SerializingCacheProvider_should_serialize_on_put_for_defaultTResult()
        {
            bool serializeInvoked = false;
            StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
                deserialize: s => s.Original
            );
            StubCacheProvider stubCacheProvider = new StubCacheProvider();
            object objectToCache = default;
            string key = "some key";

            SerializingCacheProvider<StubSerialized> serializingCacheProvider = new SerializingCacheProvider<StubSerialized>(stubCacheProvider.For<StubSerialized>(), stubSerializer);
            serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

            serializeInvoked.Should().BeTrue();

            (bool cacheHit, object fromCache) = stubCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public void Single_generic_SerializingCacheProvider_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => new StubSerialized(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );

            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            stubCacheProvider.Put(key, new StubSerialized(objectToCache), new Ttl(TimeSpan.FromMinutes(1)));

            SerializingCacheProvider<StubSerialized> serializingCacheProvider = new SerializingCacheProvider<StubSerialized>(stubCacheProvider.For<StubSerialized>(), stubSerializer);
            (bool cacheHit, object fromCache) = serializingCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            deserializeInvoked.Should().BeTrue();
            fromCache.Should().Be(objectToCache);
        }

        [Fact]
        public void Single_generic_SerializingCacheProvider_should_not_deserialize_on_get_when_item_not_in_cache()
        {
            bool deserializeInvoked = false;
            StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => new StubSerialized(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            string key = "some key";

            stubCacheProvider.TryGet(key).Item1.Should().BeFalse();

            SerializingCacheProvider<StubSerialized> serializingCacheProvider = new SerializingCacheProvider<StubSerialized>(stubCacheProvider.For<StubSerialized>(), stubSerializer);
            (bool cacheHit, object fromCache) = serializingCacheProvider.TryGet(key);

            cacheHit.Should().BeFalse();
            deserializeInvoked.Should().BeFalse();
            fromCache.Should().Be(default);
        }

        [Fact]
        public void Single_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
                deserialize: s => s.Original
            );
            StubCacheProvider stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            SerializingCacheProvider<StubSerialized> serializingCacheProvider = stubCacheProvider.For<StubSerialized>().WithSerializer(stubSerializer);
            serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

            serializeInvoked.Should().BeTrue();

            (bool cacheHit, object fromCache) = stubCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public void Single_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put_for_defaultTResult()
        {
            bool serializeInvoked = false;
            StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => { serializeInvoked = true; return new StubSerialized(o); },
                deserialize: s => s.Original
            );
            StubCacheProvider stubCacheProvider = new StubCacheProvider();
            object objectToCache = default;
            string key = "some key";

            SerializingCacheProvider<StubSerialized> serializingCacheProvider = stubCacheProvider.For<StubSerialized>().WithSerializer(stubSerializer);
            serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

            serializeInvoked.Should().BeTrue();

            (bool cacheHit, object fromCache) = stubCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public void Single_generic_SerializingCacheProvider_from_extension_syntax_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => new StubSerialized(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            object objectToCache = new object();
            string key = "some key";

            stubCacheProvider.Put(key, new StubSerialized(objectToCache), new Ttl(TimeSpan.FromMinutes(1)));

            SerializingCacheProvider<StubSerialized> serializingCacheProvider = stubCacheProvider.For<StubSerialized>().WithSerializer(stubSerializer);
            (bool cacheHit, object fromCache) = serializingCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            deserializeInvoked.Should().BeTrue();
            fromCache.Should().Be(objectToCache);
        }

        [Fact]
        public void Single_generic_SerializingCacheProvider_from_extension_syntax_should_not_deserialize_on_get_when_item_not_in_cache()
        {
            bool deserializeInvoked = false;
            StubSerializer<object, StubSerialized> stubSerializer = new StubSerializer<object, StubSerialized>(
                serialize: o => new StubSerialized(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            string key = "some key";

            stubCacheProvider.TryGet(key).Item1.Should().BeFalse();

            SerializingCacheProvider<StubSerialized> serializingCacheProvider = stubCacheProvider.For<StubSerialized>().WithSerializer(stubSerializer);
            (bool cacheHit, object fromCache) = serializingCacheProvider.TryGet(key);

            cacheHit.Should().BeFalse();
            deserializeInvoked.Should().BeFalse();
            fromCache.Should().Be(default);
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

        Action configure = () => new SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(null, stubTResultSerializer);

            configure.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("wrappedCacheProvider");
        }

        [Fact]
        public void Double_generic_constructor_should_throw_on_no_serializer()
        {
            Action configure = () => new SerializingCacheProvider<object, object>(new StubCacheProvider().For<object>(), null);

            configure.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public void Double_generic_extension_syntax_should_throw_on_no_serializer()
        {
            Action configure = () => new StubCacheProvider().For<object>().WithSerializer<object, object>(null);

            configure.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("serializer");
        }

        [Fact]
        public void Double_generic_SerializingCacheProvider_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
                deserialize: s => s.Original
            );
            var stubCacheProvider = new StubCacheProvider();
            ResultPrimitive objectToCache = ResultPrimitive.Good;
            string key = "some key";

            SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider = new SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.For<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);
            serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

            serializeInvoked.Should().BeTrue();

            (bool cacheHit, object fromCache) = stubCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized<ResultPrimitive>>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public void Double_generic_SerializingCacheProvider_should_serialize_on_put_for_defaultTResult()
        {
            bool serializeInvoked = false;
            StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
                deserialize: s => s.Original
            );
            StubCacheProvider stubCacheProvider = new StubCacheProvider();
            ResultPrimitive objectToCache = default;
            string key = "some key";

            SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider = new SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.For<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);
            serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

            serializeInvoked.Should().BeTrue();

            (bool cacheHit, object fromCache) = stubCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized<ResultPrimitive>>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public void Double_generic_SerializingCacheProvider_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => new StubSerialized<ResultPrimitive>(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            ResultPrimitive objectToCache = ResultPrimitive.Good;
            string key = "some key";

            SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider = new SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.For<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);

            stubCacheProvider.Put(key, new StubSerialized<ResultPrimitive>(objectToCache), new Ttl(TimeSpan.FromMinutes(1)));
            (bool cacheHit, object fromCache) = serializingCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            deserializeInvoked.Should().BeTrue();
            fromCache.Should().Be(objectToCache);
        }

        [Fact]
        public void Double_generic_SerializingCacheProvider_should_not_deserialize_on_get_when_item_not_in_cache()
        {
            bool deserializeInvoked = false;
            StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => new StubSerialized<ResultPrimitive>(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            string key = "some key";

            stubCacheProvider.TryGet(key).Item1.Should().BeFalse();

            SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider = new SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>>(stubCacheProvider.For<StubSerialized<ResultPrimitive>>(), stubTResultSerializer);
            (bool cacheHit, ResultPrimitive fromCache) = serializingCacheProvider.TryGet(key);

            cacheHit.Should().BeFalse();
            deserializeInvoked.Should().BeFalse();
            fromCache.Should().Be(default(ResultPrimitive));
        }

        [Fact]
        public void Double_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put()
        {
            bool serializeInvoked = false;
            StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
                deserialize: s => s.Original
            );
            var stubCacheProvider = new StubCacheProvider();
            ResultPrimitive objectToCache = ResultPrimitive.Good;
            string key = "some key";

            SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider =
                stubCacheProvider.For<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);
            serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

            serializeInvoked.Should().BeTrue();

            (bool cacheHit, object fromCache) = stubCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized<ResultPrimitive>>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public void Double_generic_SerializingCacheProvider_from_extension_syntax_should_serialize_on_put_for_defaultTResult()
        {
            bool serializeInvoked = false;
            StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => { serializeInvoked = true; return new StubSerialized<ResultPrimitive>(o); },
                deserialize: s => s.Original
            );
            StubCacheProvider stubCacheProvider = new StubCacheProvider();
            ResultPrimitive objectToCache = default;
            string key = "some key";

            SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider =
                stubCacheProvider.For<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);

            serializingCacheProvider.Put(key, objectToCache, new Ttl(TimeSpan.FromMinutes(1)));

            serializeInvoked.Should().BeTrue();

            (bool cacheHit, object fromCache) = stubCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            fromCache.Should().BeOfType<StubSerialized<ResultPrimitive>>()
                .Which.Original.Should().Be(objectToCache);
        }

        [Fact]
        public void Double_generic_SerializingCacheProvider_from_extension_syntax_should_deserialize_on_get()
        {
            bool deserializeInvoked = false;
            StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => new StubSerialized<ResultPrimitive>(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            ResultPrimitive objectToCache = ResultPrimitive.Good;
            string key = "some key";

            SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider =
                stubCacheProvider.For<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);

            stubCacheProvider.Put(key, new StubSerialized<ResultPrimitive>(objectToCache), new Ttl(TimeSpan.FromMinutes(1)));
            (bool cacheHit, ResultPrimitive fromCache) = serializingCacheProvider.TryGet(key);

            cacheHit.Should().BeTrue();
            deserializeInvoked.Should().BeTrue();
            fromCache.Should().Be(objectToCache);
        }

        [Fact]
        public void Double_generic_SerializingCacheProvider_from_extension_syntax_should_not_deserialize_on_get_when_item_not_in_cache()
        {
            bool deserializeInvoked = false;
            StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>> stubTResultSerializer = new StubSerializer<ResultPrimitive, StubSerialized<ResultPrimitive>>(
                serialize: o => new StubSerialized<ResultPrimitive>(o),
                deserialize: s => { deserializeInvoked = true; return s.Original; }
            );
            var stubCacheProvider = new StubCacheProvider();
            string key = "some key";

            stubCacheProvider.TryGet(key).Item2.Should().BeNull();

            SerializingCacheProvider<ResultPrimitive, StubSerialized<ResultPrimitive>> serializingCacheProvider =
                stubCacheProvider.For<StubSerialized<ResultPrimitive>>().WithSerializer(stubTResultSerializer);
            (bool cacheHit, ResultPrimitive fromCache) = serializingCacheProvider.TryGet(key);

            cacheHit.Should().BeFalse();
            deserializeInvoked.Should().BeFalse();
            fromCache.Should().Be(default(ResultPrimitive));
        }

        #endregion
    }
}
