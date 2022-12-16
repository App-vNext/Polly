using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Polly.Specs.Helpers
{
    public static class ObjectExtensions
    {
        public static IDictionary<string, object> AsDictionary(this object source)
        {
            return source.GetType().GetRuntimeProperties().ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );
        }
    }
}