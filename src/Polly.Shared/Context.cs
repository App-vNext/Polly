using System;
using System.Collections.Generic;

namespace Polly
{
    /// <summary>
    /// Context that carries with a single execution through a Policy.   Commonly-used properties are directly on the class.  Backed by a dictionary of string key / object value pairs, to which user-defined values may be added.
    /// <remarks>Do not re-use an instance of <see cref="Context"/> across more than one execution.</remarks>
    /// </summary>
    public class Context : Dictionary<string, object>
    {
        // For an individual execution through a policy or policywrap, it is expected that all execution steps (for example executing the user delegate, invoking policy-activity delegates such as onRetry, onBreak, onTimeout etc) execute sequentially.  
        // Therefore, this class is intentionally not constructed to be safe for concurrent access from multiple threads.

        private static readonly IDictionary<string, object> emptyDictionary = new Dictionary<string, object>();
        internal static readonly Context None = new Context(emptyDictionary);

        private Guid? _executionGuid;

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class, with the specified <paramref name="executionKey"/>.
        /// </summary>
        /// <param name="executionKey">The execution key.</param>
        public Context(String executionKey) : this(executionKey, emptyDictionary)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class, with the specified <paramref name="executionKey" /> and the supplied <paramref name="contextData"/>.
        /// </summary>
        /// <param name="executionKey">The execution key.</param>
        /// <param name="contextData">The context data.</param>
        public Context(String executionKey, IDictionary<string, object> contextData) : this(contextData)
        {
            ExecutionKey = executionKey;
        }

        internal Context() : this(emptyDictionary)
        {
        }

        internal Context(IDictionary<string, object> contextData) : base(contextData)
        {
            if (contextData == null) throw new ArgumentNullException(nameof(contextData));
        }

        /// <summary>
        /// A key unique to the outermost <see cref="Polly.Wrap.PolicyWrap"/> instance involved in the current PolicyWrap execution.
        /// </summary>
        public String PolicyWrapKey { get; internal set; }

        /// <summary>
        /// A key unique to the <see cref="Policy"/> instance executing the current delegate.
        /// </summary>
        public String PolicyKey { get; internal set; }

        /// <summary>
        /// A key unique to the call site of the current execution. 
        /// <remarks>The value is set </remarks>
        /// </summary>
        public String ExecutionKey { get; }

        /// <summary>
        /// A Guid guaranteed to be unique to each execution.
        /// </summary>
        public Guid ExecutionGuid
        {
            get
            {
                if (!_executionGuid.HasValue) { _executionGuid = Guid.NewGuid(); }
                return _executionGuid.Value;
            }
        }
        
    }
}