using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Common;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs
{
    public class AfterFinalRetryFailureSpecs
    {


        [Fact]
        public void After_Final_Retry_Failure_Should_Happen_After_Final_Retry()
        {
            var didAfterFinalRetryFailureRunLast = false;
            Policy.Handle<DivideByZeroException>()
                .AfterFinalRetryFailure(ex => didAfterFinalRetryFailureRunLast = true)
                .Retry(2, (exception, i) => didAfterFinalRetryFailureRunLast = false)
                .Invoking(policy => policy.RaiseException<DivideByZeroException>(2+1))
                .Invoke();

            didAfterFinalRetryFailureRunLast.ShouldBeEquivalentTo(true);
        }
    }
}
