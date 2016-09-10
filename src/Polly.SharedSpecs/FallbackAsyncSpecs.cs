﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Fallback;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

using Scenario = Polly.Specs.Helpers.PolicyExtensionsAsync.ExceptionAndOrCancellationScenario;

namespace Polly.Specs
{
    public class FallbackAsyncSpecs
    {
        #region Configuration guard condition tests

        [Fact]
        public void Should_throw_when_fallback_func_is_null()
        {
            Func<CancellationToken, Task> fallbackActionAsync  = null;

            Action policy = () => Policy
                .Handle<DivideByZeroException>()
                .FallbackAsync(fallbackActionAsync);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_func_is_null_with_onFallback()
        {
            Func<CancellationToken, Task> fallbackActionAsync  = null;
            Func<Exception, Task> onFallbackAsync = _ => TaskHelper.EmptyTask;

            Action policy = () => Policy
                .Handle<DivideByZeroException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_func_is_null_with_onFallback_with_context()
        {
            Func<CancellationToken, Task> fallbackActionAsync  = null;
            Func<Exception, Context, Task> onFallbackAsync = (_, __) => TaskHelper.EmptyTask;

            Action policy = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null()
        {
            Func<CancellationToken, Task> fallbackActionAsync  = ct => TaskHelper.EmptyTask;
            Func<Exception, Task> onFallbackAsync = null;

            Action policy = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onFallbackAsync");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null_with_context()
        {
            Func<CancellationToken, Task> fallbackActionAsync  = ct => TaskHelper.EmptyTask;
            Func<Exception, Context, Task> onFallbackAsync = null;

            Action policy = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onFallbackAsync");
        }

        #endregion

        #region Policy operation tests

        [Fact]
        public void Should_not_execute_fallback_when_execute_delegate_does_not_throw()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync  = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.ExecuteAsync(() => TaskHelper.EmptyTask);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_not_execute_fallback_when_execute_delegate_throws_exception_not_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync  = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(x => x.RaiseExceptionAsync<ArgumentNullException>()).ShouldThrow<ArgumentNullException>();

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_execute_fallback_when_execute_delegate_throws_exception_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync  = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>()).ShouldNotThrow();

            fallbackActionExecuted.Should().BeTrue();
        }


        [Fact]
        public void Should_execute_fallback_when_execute_delegate_throws_one_of_exceptions_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync  = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Or<ArgumentException>()
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>()).ShouldNotThrow();

            fallbackActionExecuted.Should().BeTrue();
        }


        [Fact]
        public void Should_not_execute_fallback_when_execute_delegate_throws_exception_not_one_of_exceptions_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync  = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Or<NullReferenceException>()
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(x => x.RaiseExceptionAsync<ArgumentNullException>()).ShouldThrow<ArgumentNullException>();

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_not_execute_fallback_when_exception_thrown_does_not_match_handling_predicates()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync  = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>(e => false)
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>()).ShouldThrow<DivideByZeroException>();

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_not_execute_fallback_when_exception_thrown_does_not_match_any_of_handling_predicates()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync  = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>(e => false)
                                    .Or<ArgumentNullException>(e => false)
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>()).ShouldThrow<DivideByZeroException>();

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_execute_fallback_when_exception_thrown_matches_handling_predicates()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync  = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>(e => true)
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>()).ShouldNotThrow();

            fallbackActionExecuted.Should().BeTrue();
        }


        [Fact]
        public void Should_execute_fallback_when_exception_thrown_matches_one_of_handling_predicates()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync  = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>(e => true)
                                    .Or<ArgumentNullException>()
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>()).ShouldNotThrow();

            fallbackActionExecuted.Should().BeTrue();
        }

        [Fact]
        public void Should_not_handle_exception_thrown_by_fallback_delegate_even_if_is_exception_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync  = _ =>
            {
                fallbackActionExecuted = true;
                throw new DivideByZeroException() { HelpLink = "FromFallbackAction" };
            };

            FallbackPolicy fallbackPolicy = Policy
                .Handle<DivideByZeroException>()
                .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>((e, i) => e.HelpLink = "FromExecuteDelegate"))
                .ShouldThrow<DivideByZeroException>().And.HelpLink.Should().Be("FromFallbackAction");

            fallbackActionExecuted.Should().BeTrue();
        }

        #endregion

        #region onPolicyEvent delegate tests

        [Fact]
        public void Should_call_onFallback_passing_exception_triggering_fallback()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            Exception exceptionPassedToOnFallback = null;
            Func<Exception, Task> onFallbackAsync = ex => { exceptionPassedToOnFallback = ex; return TaskHelper.EmptyTask; };

            FallbackPolicy fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            Exception instanceToThrow = new ArgumentNullException("myParam");
            fallbackPolicy.ExecuteAsync(() => { throw instanceToThrow; });

            fallbackActionExecuted.Should().BeTrue();
            exceptionPassedToOnFallback.Should().BeOfType<ArgumentNullException>();
            exceptionPassedToOnFallback.Should().Be(instanceToThrow);
        }

        [Fact]
        public void Should_not_call_onFallback_when_execute_delegate_does_not_throw()
        {
            Func<CancellationToken, Task> fallbackActionAsync = _ => TaskHelper.EmptyTask;

            bool onFallbackExecuted = false;
            Func<Exception, Task> onFallbackAsync = ex => { onFallbackExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy fallbackPolicy = Policy
                .Handle<DivideByZeroException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            fallbackPolicy.ExecuteAsync(() => TaskHelper.EmptyTask);

            onFallbackExecuted.Should().BeFalse();
        }

        #endregion

        #region Context passing tests

        [Fact]
        public void Should_call_onFallback_with_the_passed_context()
        {
            Func<CancellationToken, Task> fallbackActionAsync = _ => TaskHelper.EmptyTask;

            IDictionary<string, object> contextData = null;

            Func<Exception, Context, Task> onFallbackAsync = (ex, ctx) => { contextData = ctx; return TaskHelper.EmptyTask; };

            FallbackPolicy fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            fallbackPolicy.Awaiting(p => p.ExecuteAsync(() => { throw new ArgumentNullException(); },
                new { key1 = "value1", key2 = "value2" }.AsDictionary()))
                .ShouldNotThrow();

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Should_call_onFallback_with_the_passed_context_when_execute_and_capture()
        {
            Func<CancellationToken, Task> fallbackActionAsync = _ => TaskHelper.EmptyTask;

            IDictionary<string, object> contextData = null;

            Func<Exception, Context, Task> onFallbackAsync = (ex, ctx) => { contextData = ctx; return TaskHelper.EmptyTask; };

            FallbackPolicy fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            fallbackPolicy.Awaiting(p => p.ExecuteAndCaptureAsync(() => { throw new ArgumentNullException(); },
                new { key1 = "value1", key2 = "value2" }.AsDictionary()))
                .ShouldNotThrow();

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Should_call_onFallback_with_independent_context_for_independent_calls()
        {
            Func<CancellationToken, Task> fallbackActionAsync = _ => TaskHelper.EmptyTask;

            IDictionary<Type, object> contextData = new Dictionary<Type, object>();

            Func<Exception, Context, Task> onFallbackAsync = (ex, ctx) => { contextData[ex.GetType()] = ctx["key"]; return TaskHelper.EmptyTask; };

            FallbackPolicy fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .Or<DivideByZeroException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            fallbackPolicy.Awaiting(
                p => p.ExecuteAsync(() => { throw new ArgumentNullException(); }, new { key = "value1" }.AsDictionary()))
                .ShouldNotThrow();

            fallbackPolicy.Awaiting(
                p => p.ExecuteAsync(() => { throw new DivideByZeroException(); }, new { key = "value2" }.AsDictionary()))
                .ShouldNotThrow();

            contextData.Count.Should().Be(2);
            contextData.Keys.Should().Contain(typeof(ArgumentNullException));
            contextData.Keys.Should().Contain(typeof(DivideByZeroException));
            contextData[typeof(ArgumentNullException)].Should().Be("value1");
            contextData[typeof(DivideByZeroException)].Should().Be("value2");

        }

        [Fact]
        public void Context_should_be_empty_if_execute_not_called_with_any_context_data()
        {
            Context capturedContext = null;
            bool onFallbackExecuted = false;

            Func<CancellationToken, Task> fallbackActionAsync = _ => TaskHelper.EmptyTask;
            Func<Exception, Context, Task> onFallbackAsync = (ex, ctx) => { onFallbackExecuted = true; capturedContext = ctx; return TaskHelper.EmptyTask; };


            FallbackPolicy fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .Or<DivideByZeroException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            fallbackPolicy.RaiseExceptionAsync<DivideByZeroException>();

            onFallbackExecuted.Should().BeTrue();
            capturedContext.Should().BeEmpty();
        }

        #endregion

        #region Cancellation tests

        [Fact]
        public void Should_execute_action_when_non_faulting_and_cancellationtoken_not_cancelled()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null,
            };

            policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldNotThrow();
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_execute_fallback_when_faulting_and_cancellationtoken_not_cancelled()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = null,
            };

            policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldNotThrow();
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeTrue();
        }

        [Fact]
        public void Should_not_execute_action_when_cancellationtoken_cancelled_before_execute()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
            };

            cancellationTokenSource.Cancel();

            policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
            attemptsInvoked.Should().Be(0);

            fallbackActionExecuted.Should().BeFalse();

        }

        [Fact]
        public void Should_report_cancellation_and_not_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationtoken()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_report_cancellation_and_not_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_does_not_observe_cancellationtoken()
        {

            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = false
            };

            policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_report_cancellation_after_faulting_action_execution_and_not_call_fallback_function_if_onFallback_invokes_cancellation()
        {

            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };


            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Func<Exception, Context, Task> onFallbackAsync = (_, __) => { cancellationTokenSource.Cancel(); return TaskHelper.EmptyTask; };

            FallbackPolicy policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = null, // Cancellation during onFallback instead - see above.
                ActionObservesCancellation = false
            };

            policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeFalse();
        }

        #endregion


    }
}
