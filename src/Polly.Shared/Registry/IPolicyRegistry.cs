using System;
using System.Collections.Generic;
using System.Text;

namespace Polly.Registry
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Policy"></typeparam>
    public interface IPolicyRegistry<Key, Policy> : IDictionary<Key, Policy> where Policy: Polly.Policy
    {
    }
}
