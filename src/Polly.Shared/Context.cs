using System;
using System.Collections.Generic;

namespace Polly
{
    /// <summary>
    /// Context that carries with a single execution through a Policy.   Commonly-used properties are directly on the class.  Backed by a dictionary of string key / object value pairs, to which user-defined values may be added.
    /// <remarks>Do not re-use an instance of <see cref="Context"/> across more than one execution.</remarks>
    /// </summary>
    public partial class Context
    {
        internal static readonly Context None = new Context();

        private Guid? _executionGuid;

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class, with the specified <paramref name="executionKey"/>.
        /// </summary>
        /// <param name="executionKey">The execution key.</param>
        public Context(String executionKey)
        {
            ExecutionKey = executionKey;
        }

        internal Context()
        {
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