using System;
using System.Collections.Generic;
using FluentAssertions;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs
{
    public class PolicyTResultSpecs
    {
        #region Execute tests

        [Fact]
        public void Executing_the_policy_function_should_execute_the_specified_function_and_return_the_result()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Retry((_, __) => { });

            var result = policy.Execute(() => ResultPrimitive.Good);

            result.Should()
                .Be(ResultPrimitive.Good);
        }

        #endregion

        #region ExecuteAndCapture tests

        [Fact]
        public void Executing_the_policy_function_successfully_should_return_success_result()
        {
            var result = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Retry((_, __) => { })
                .ExecuteAndCapture(() => ResultPrimitive.Good);

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Successful,
                FinalException = (Exception)null,
                ExceptionType = (ExceptionType?)null,
                Result = ResultPrimitive.Good,
                FinalHandledResult = default(ResultPrimitive),
                FaultType = (FaultType?)null
            }, options => options.Excluding(o => o.Context));
        }

        [Fact]
        public void Executing_the_policy_function_and_failing_with_a_handled_result_should_return_failure_result_indicating_that_result_is_one_handled_by_this_policy()
        {
            var handledResult = ResultPrimitive.Fault;

            var result = Policy
                .HandleResult(handledResult)
                .Retry((_, __) => { })
                .ExecuteAndCapture(() => handledResult);

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = (Exception)null,
                ExceptionType = (ExceptionType?)null,
                FaultType = FaultType.ResultHandledByThisPolicy,
                FinalHandledResult = handledResult,
                Result = default(ResultPrimitive)
            }, options => options.Excluding(o => o.Context));
        }

        [Fact]
        public void Executing_the_policy_function_and_returning_an_unhandled_result_should_return_result_not_indicating_any_failure()
        {
            var handledResult = ResultPrimitive.Fault;
            var unhandledResult = ResultPrimitive.Good;

            var result = Policy
                .HandleResult(handledResult)
                .Retry((_, __) => { })
                .ExecuteAndCapture(() => unhandledResult);

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Successful,
                FinalException = (Exception)null,
                ExceptionType = (ExceptionType?)null,
                Result = unhandledResult,
                FinalHandledResult = default(ResultPrimitive),
                FaultType = (FaultType?)null
            }, options => options.Excluding(o => o.Context));
        }

        #endregion

        #region Context tests
        

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(ctx => ResultPrimitive.Good, (IDictionary<string, object>)null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(ctx => ResultPrimitive.Good, (Context)null))
                .ShouldThrow<ArgumentNullException>().And
                .ParamName.Should().Be("context");
        }

        [Fact]
        public void Executing_the_policy_function_should_pass_context_to_executed_delegate()
        {
            string operationKey = "SomeKey";
            Context executionContext = new Context(operationKey);
            Context capturedContext = null;

            Policy<ResultPrimitive> policy = Policy.NoOp<ResultPrimitive>();

            policy.Execute((context) => { capturedContext = context; return ResultPrimitive.Good; }, executionContext);

            capturedContext.Should().BeSameAs(executionContext);
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(ctx => ResultPrimitive.Good, (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(ctx => ResultPrimitive.Good, (Context)null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_pass_context_to_executed_delegate()
        {
            string operationKey = "SomeKey";
            Context executionContext = new Context(operationKey);
            Context capturedContext = null;

            Policy<ResultPrimitive> policy = Policy.NoOp<ResultPrimitive>();

            policy.ExecuteAndCapture((context) => { capturedContext = context; return ResultPrimitive.Good; }, executionContext);

            capturedContext.Should().BeSameAs(executionContext);
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_pass_context_to_PolicyResult()
        {
            string operationKey = "SomeKey";
            Context executionContext = new Context(operationKey);

            Policy<ResultPrimitive> policy = Policy.NoOp<ResultPrimitive>();

            policy.ExecuteAndCapture((context) => ResultPrimitive.Good, executionContext)
                .Context.Should().BeSameAs(executionContext);
        }

        #endregion
    }
}