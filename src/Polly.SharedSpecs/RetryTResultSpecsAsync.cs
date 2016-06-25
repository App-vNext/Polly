using System;
using System.Collections.Generic;
using System.Text;

namespace Polly.Specs
{
    public class RetryTResultSpecsAsync
    {
        // Contains stubs of async tests that were wrongly placed in the RetrySpecs test class (for sync policy).  Will be converted to RetryTResultSpecsAsync -style tests in subsequent commit.

        //[Fact]
        //public void Should_not_throw_when_specified_result_raised_less_number_of_times_than_retry_count_async()
        //{
        //    var policy = Policy
        //        .HandleResult(SomeResult.A)
        //        .RetryAsync(3);

        //    policy.Awaiting(x => x.RaiseResultSequenceAsync<DivideByZeroException>())
        //          .ShouldNotThrow();
        //}

        //[Fact]
        //public void Should_not_throw_when_one_of_the_specified_results_raised_less_number_of_times_than_retry_count_async()
        //{
        //    var policy = Policy
        //        .HandleResult(SomeResult.A)
        //        .Or<ArgumentException>()
        //        .RetryAsync(3);

        //    policy.Awaiting(x => x.RaiseResultSequenceAsync<ArgumentException>())
        //          .ShouldNotThrow();
        //}

        //[Fact]
        //public void Should_return_handled_result_when_handled_result_raised_more_times_then_retry_count_async()
        //{
        //    var policy = Policy
        //        .HandleResult(SomeResult.A)
        //        .RetryAsync(3);

        //    policy.Awaiting(x => x.RaiseResultSequenceAsync<DivideByZeroException>(3 + 1))
        //          .ShouldThrow<DivideByZeroException>();
        //}

        //[Fact]
        //public void Should_return_handled_result_when_one_of_the_handled_results_are_raised_more_times_then_retry_count_async()
        //{
        //    var policy = Policy
        //        .HandleResult(SomeResult.A)
        //        .Or<ArgumentException>()
        //        .RetryAsync(3);

        //    policy.Awaiting(x => x.RaiseResultSequenceAsync<DivideByZeroException>(3 + 1))
        //          .ShouldThrow<DivideByZeroException>();
        //}

    }
}
