using System;
using System.Collections.Generic;
using Polly.Wrap;

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
        /// When execution is through a <see cref="PolicyWrap"/>, identifies the PolicyWrap executing the current delegate by returning the <see cref="Policy.PolicyKey"/> of the outermost layer in the PolicyWrap; otherwise, null.
        /// </summary>
        public String PolicyWrapKey { get; internal set; }

        /// <summary>
        /// The <see cref="Policy.PolicyKey"/> of the <see cref="Policy"/> instance executing the current delegate.
        /// </summary>
        public String PolicyKey { get; internal set; }

        /// <summary>
        /// A key unique to the call site of the current execution. 
        /// <remarks><see cref="Policy"/> instances are commonly reused across multiple call sites.  Set an ExecutionKey so that logging and metrics can distinguish usages of policy instances at different call sites.</remarks>
        /// <remarks>The value is set by using the <see cref="Context(String)"/> constructor taking an executionKey parameter.</remarks>
        /// </summary>
        public String ExecutionKey { get; }

        /// <summary>
        /// A Guid guaranteed to be unique to each execution.
        /// <remarks>Acts as a correlation id so that events specific to a single execution can be identified in logging and telemetry.</remarks>
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
