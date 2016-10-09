using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs
{
    public class ContextualPolicyAsyncSpecs
    {

        [Fact]
        public void Executing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAsync(() => TaskHelper.EmptyTask, (IDictionary<string, object>) null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAsync(() => Task.FromResult(2), (IDictionary<string, object>) null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_action_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAsync(() => TaskHelper.EmptyTask, (Context) null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAsync(() => Task.FromResult(2), (Context)null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAndCaptureAsync(() => TaskHelper.EmptyTask, (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAndCaptureAsync(() => Task.FromResult(2), (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Execute_and_capturing_the_policy_action_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAndCaptureAsync(() => TaskHelper.EmptyTask, (Context)null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAndCaptureAsync(() => Task.FromResult(2), (Context)null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }
    }
}
