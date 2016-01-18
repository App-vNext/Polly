using System.Collections.Generic;

#if SUPPORTS_READONLY_COLLECTION
using System.Collections.ObjectModel;
#else
using Polly.Utilities;
#endif

namespace Polly
{
    /// <summary>
    /// A readonly dictionary of string key / object value pairs
    /// </summary>
    public class Context : ReadOnlyDictionary<string, object>
    {
        internal static readonly Context Empty = new Context(new Dictionary<string, object>());

        internal Context(IDictionary<string, object> values) : base(values)
        {
        }
    }
}