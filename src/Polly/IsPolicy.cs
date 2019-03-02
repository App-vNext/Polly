﻿using System;

namespace Polly
{
    /// <summary>
    /// A marker interface identifying Polly policies of all types, and containing properties common to all policies
    /// </summary>
    public interface IsPolicy
    {    
        /// <summary>
        /// A key intended to be unique to each policy instance, which is passed with executions as the <see cref="M:Context.PolicyKey"/> property.
        /// </summary>
        String PolicyKey { get; }
    }
}
