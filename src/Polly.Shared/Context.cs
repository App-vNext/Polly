using System;
using System.Collections.Generic;
using Polly.Wrap;
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
        private static readonly IDictionary<string, object> emptyDictionary = new Dictionary<string, object>();
        internal static readonly Context None = new Context(emptyDictionary);

        private string _policyKey;
        private string _policyWrapKey;
        private readonly string _executionKey;
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
            _executionKey = executionKey;
        }

        internal Context() : this(emptyDictionary)
        {
        }

        internal Context(IDictionary<string, object> contextData) : base(contextData)
        {
            if (contextData == null) throw new ArgumentNullException(nameof(contextData));
        }

        /// <summary>
        /// A key unique to the outermost <see cref="PolicyWrap"/> instance involved in the current PolicyWrap execution.
        /// </summary>
        public String PolicyWrapKey => _policyWrapKey;

        /// <summary>
        /// A key unique to the <see cref="Policy"/> instance executing the current delegate.
        /// </summary>
        public String PolicyKey => _policyKey;

        /// <summary>
        /// A key unique to the call site of the current execution. 
        /// <remarks>The value is set </remarks>
        /// </summary>
        public String ExecutionKey => _executionKey;

        /// <summary>
        /// A Guid guaranteed to be unique to each execution.
        /// </summary>
        public Guid ExecutionGuid
        {
            get
            {
                if (_executionGuid == null) { _executionGuid = Guid.NewGuid(); }
                return _executionGuid.Value;
            }
        }

        internal void SetPolicyContext(Policy executingPolicy)
        {
            _policyKey = executingPolicy.PolicyKey;

            if (PolicyWrapKey == null && executingPolicy is PolicyWrap)
            {
                _policyWrapKey = executingPolicy.PolicyKey;
            }
        }

        internal void SetPolicyContext<TResult>(Policy<TResult> executingPolicy)
        {
            _policyKey = executingPolicy.PolicyKey;

            if (PolicyWrapKey == null && executingPolicy is PolicyWrap<TResult>)
            {
                _policyWrapKey = executingPolicy.PolicyKey;
            }
        }
    }
}