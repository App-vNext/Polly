using System;
using System.Collections.Generic;
using System.Linq;

namespace Polly
{
    /// <summary>
    /// A collection of predicates used to define whether a policy handles a given <see cref="Exception"/>.
    /// </summary>
    public class ExceptionPredicates
    {
        private List<ExceptionPredicate> _predicates;

        internal void Add(ExceptionPredicate predicate)
        {
            _predicates = _predicates ?? new List<ExceptionPredicate>(); // The ?? pattern here is sufficient; only a deliberately contrived example would lead to the same PolicyBuilder instance being used in a multi-threaded way to define policies simultaneously on multiple threads.

            _predicates.Add(predicate);
        }

        /// <summary>
        /// Assess whether the passed <see cref="Exception"/>, <paramref name="ex"/>, matches any of the predicates.
        /// <remarks>If the .HandleInner() method was used when configuring the policy, predicates may test whether any inner exceptions of <paramref name="ex"/> match and may return a matching inner exception.</remarks>
        /// </summary>
        /// <param name="ex">The exception to assess against the predicates.</param>
        /// <returns>The first exception to match a predicate; or null, if no match is found.</returns>
        public Exception FirstMatchOrDefault(Exception ex) => _predicates?.Select(predicate => predicate(ex)).FirstOrDefault(e => e != null);

        /// <summary>
        /// Specifies that no Exception-handling filters are applied or are required.
        /// </summary>
        public static readonly ExceptionPredicates None = new ExceptionPredicates();
    }

}