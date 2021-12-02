using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Polly.Specs
{
    public class PolicySpecs
    {
        #region Execute tests

        [Fact]
        public void Executing_the_policy_action_should_execute_the_specified_action()
        {
            var executed = false;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _) => { });

            policy.Execute(() => executed = true);

            executed.Should()
                .BeTrue();
        }

        [Fact]
        public void Executing_the_policy_function_should_execute_the_specified_function_and_return_the_result()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _) => { });

            var result = policy.Execute(() => 2);

            result.Should()
                .Be(2);
        }

        #endregion

        #region ExecuteAndCapture tests

        [Fact]
        public void Executing_the_policy_action_successfully_should_return_success_result()
        {
            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _) => { })
                .ExecuteAndCapture(() => { });

            result.Should().BeEquivalentTo(new
            {
                Outcome = OutcomeType.Successful,
                FinalException = (Exception) null,
                ExceptionType = (ExceptionType?) null,
            });
        }

        [Fact]
        public void Executing_the_policy_action_and_failing_with_a_handled_exception_type_should_return_failure_result_indicating_that_exception_type_is_one_handled_by_this_policy()
        {
            var handledException = new DivideByZeroException();

            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _) => { })
                .ExecuteAndCapture(() => throw handledException);

            result.Should().BeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = handledException,
                ExceptionType = ExceptionType.HandledByThisPolicy
            });
        }

        [Fact]
        public void Executing_the_policy_action_and_failing_with_an_unhandled_exception_type_should_return_failure_result_indicating_that_exception_type_is_unhandled_by_this_policy()
        {
            var unhandledException = new Exception();

            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _) => { })
                .ExecuteAndCapture(() => throw unhandledException);

            result.Should().BeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = unhandledException,
                ExceptionType = ExceptionType.Unhandled
            });
        }

        [Fact]
        public void Executing_the_policy_function_successfully_should_return_success_result()
        {
            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _) => { })
                .ExecuteAndCapture(() => Int32.MaxValue);

            result.Should().BeEquivalentTo(new
            {
                Outcome = OutcomeType.Successful,
                FinalException = (Exception)null,
                ExceptionType = (ExceptionType?)null,
                FaultType = (FaultType?)null,
                FinalHandledResult = default(int),
                Result = Int32.MaxValue
            });
        }

        [Fact]
        public void Executing_the_policy_function_and_failing_with_a_handled_exception_type_should_return_failure_result_indicating_that_exception_type_is_one_handled_by_this_policy()
        {
            var handledException = new DivideByZeroException();

            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _) => { })
                .ExecuteAndCapture<int>(() => throw handledException);

            result.Should().BeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = handledException,
                ExceptionType = ExceptionType.HandledByThisPolicy,
                FaultType = FaultType.ExceptionHandledByThisPolicy,
                FinalHandledResult = default(int),
                Result = default(int)
            });
        }

        [Fact]
        public void Executing_the_policy_function_and_failing_with_an_unhandled_exception_type_should_return_failure_result_indicating_that_exception_type_is_unhandled_by_this_policy()
        {
            var unhandledException = new Exception();

            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _) => { })
                .ExecuteAndCapture<int>(() => throw unhandledException);

            result.Should().BeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = unhandledException,
                ExceptionType = ExceptionType.Unhandled,
                FaultType = FaultType.UnhandledException,
                FinalHandledResult = default(int),
                Result = default(int)
            });
        }

        #endregion

        #region Context tests

        [Fact]
        public void Executing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _, _) => { });

            policy.Invoking(p => p.Execute(_ => { }, (IDictionary<string, object>)null))
                  .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_action_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _, _) => { });

            policy.Invoking(p => p.Execute(_ => { }, (Context)null))
                .Should().Throw<ArgumentNullException>().And
                .ParamName.Should().Be("context");
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _, _) => { });

            policy.Invoking(p => p.Execute(_ => 2, (IDictionary<string, object>)null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _, _) => { });

            policy.Invoking(p => p.Execute(_ => 2, (Context)null))
                .Should().Throw<ArgumentNullException>().And
                .ParamName.Should().Be("context");
        }

        [Fact]
        public void Executing_the_policy_function_should_pass_context_to_executed_delegate()
        {
            string operationKey = "SomeKey";
            Context executionContext = new Context(operationKey);
            Context capturedContext = null;

            Policy policy = Policy.NoOp();

            policy.Execute(context => { capturedContext = context; }, executionContext);

            capturedContext.Should().BeSameAs(executionContext);
        }

        [Fact]
        public void Execute_and_capturing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _, _) => { });

            policy.Invoking(p => p.ExecuteAndCapture(_ => { }, (IDictionary<string, object>)null))
                  .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Execute_and_capturing_the_policy_action_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _, _) => { });

            policy.Invoking(p => p.ExecuteAndCapture(_ => { }, (Context)null))
                .Should().Throw<ArgumentNullException>().And
                .ParamName.Should().Be("context");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _, _) => { });

            policy.Invoking(p => p.ExecuteAndCapture(_ => 2, (IDictionary<string, object>)null))
                  .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, _, _) => { });

            policy.Invoking(p => p.ExecuteAndCapture(_ => 2, (Context)null))
                  .Should().Throw<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_pass_context_to_executed_delegate()
        {
            string operationKey = "SomeKey";
            Context executionContext = new Context(operationKey);
            Context capturedContext = null;

            Policy policy = Policy.NoOp();

            policy.ExecuteAndCapture(context => { capturedContext = context; }, executionContext);

            capturedContext.Should().BeSameAs(executionContext);
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_pass_context_to_PolicyResult()
        {
            string operationKey = "SomeKey";
            Context executionContext = new Context(operationKey);

            Policy policy = Policy.NoOp();

            policy.ExecuteAndCapture(_ => { }, executionContext)
                .Context.Should().BeSameAs(executionContext);
        }

        #endregion
    }
}