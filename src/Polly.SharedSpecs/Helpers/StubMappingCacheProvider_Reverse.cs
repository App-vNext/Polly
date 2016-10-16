using System;
using System.Linq;
using Polly.Caching;

namespace Polly.Specs.Helpers
{
    internal class StubMappingCacheProvider_Reverse : MappingCacheProvider
    {
        public StubMappingCacheProvider_Reverse(ICacheProvider wrappedCacheProvider) : base(wrappedCacheProvider)
        { }

        public override object GetMapper(object rawObjectFromCache)
        {
            if (rawObjectFromCache == null) return null;

            return new string(((string)rawObjectFromCache).ToCharArray().Reverse().ToArray());
        }

        public override object PutMapper(object nativeObject)
        {
            if (nativeObject == null) return null;

            return new string(((string)nativeObject).ToCharArray().Reverse().ToArray());
        }
    }

    internal class TypedStubMappingCacheProvider_Reverse : MappingCacheProvider<String, String>
    {
        public TypedStubMappingCacheProvider_Reverse(ICacheProvider<string> wrappedCacheProvider) : base(wrappedCacheProvider)
        { }

        public override string GetMapper(string rawObjectFromCache)
        {
            if (rawObjectFromCache == null) return null;

            return new string(rawObjectFromCache.ToCharArray().Reverse().ToArray());
        }

        public override string PutMapper(string nativeObject)
        {
            if (nativeObject == null) return null;

            return new string(nativeObject.ToCharArray().Reverse().ToArray());
        }
    }
}
