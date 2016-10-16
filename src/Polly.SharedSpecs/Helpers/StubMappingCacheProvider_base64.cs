using Polly.Caching;

namespace Polly.Specs.Helpers
{
    internal class StubMappingCacheProvider_base64 : MappingCacheProvider
    {
        public StubMappingCacheProvider_base64(ICacheProvider wrappedCacheProvider) : base(wrappedCacheProvider)
        { }

        public override object GetMapper(object rawObjectFromCache)
        {
            if (rawObjectFromCache == null) return null;

            byte[] base64EncodedBytes = System.Convert.FromBase64String((string)rawObjectFromCache);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes, 0, base64EncodedBytes.Length);
        }

        public override object PutMapper(object nativeObject)
        {
            if (nativeObject == null) return null;

            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes((string)nativeObject);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }


    internal class TypedStubMappingCacheProvider_base64 : MappingCacheProvider<string, string>
    {
        public TypedStubMappingCacheProvider_base64(ICacheProvider<string> wrappedCacheProvider) : base(wrappedCacheProvider)
        { }

        public override string GetMapper(string rawObjectFromCache)
        {
            if (rawObjectFromCache == null) return null;

            byte[] base64EncodedBytes = System.Convert.FromBase64String((string)rawObjectFromCache);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes, 0, base64EncodedBytes.Length);
        }

        public override string PutMapper(string nativeObject)
        {
            if (nativeObject == null) return null;

            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes((string)nativeObject);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
