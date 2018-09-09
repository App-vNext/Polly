using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Polly.Utilities
{
    internal class PredicateHelper
    {
        public static readonly IEnumerable<ExceptionPredicate> EmptyExceptionPredicates = Enumerable.Empty<ExceptionPredicate>();
    }

    internal class PredicateHelper<TResult>
    {
        public static readonly IEnumerable<ResultPredicate<TResult>> EmptyResultPredicates = Enumerable.Empty<ResultPredicate<TResult>>();
    }
}
