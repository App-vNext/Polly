﻿using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Polly.Fallback;
using Polly.Specs.Helpers;
using Xunit;

using Scenario = Polly.Specs.Helpers.PolicyExtensions.ExceptionAndOrCancellationScenario;

namespace Polly.Specs
{
    public class FallbackSpecs
    {
        #region Configuration guard condition tests

        [Fact]
        public void Should_throw_when_fallback_action_is_null()
        {
            Action fallbackAction = null;

            Action policy = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_action_with_cancellation_is_null()
        {
            Action<CancellationToken> fallbackAction = null;

            Action policy = () => Policy
                .Handle<DivideByZeroException>()
                .Fallback(fallbackAction);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_action_is_null_with_onFallback()
        {
            Action fallbackAction = null;
            Action<Exception> onFallback = _ => { };

            Action policy = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_action_with_cancellation_is_null_with_onFallback()
        {
            Action<CancellationToken> fallbackAction = null;
            Action<Exception> onFallback = _ => { };

            Action policy = () => Policy
                .Handle<DivideByZeroException>()
                .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_action_is_null_with_onFallback_with_context()
        {
            Action fallbackAction = null;
            Action<Exception, Context> onFallback = (_, __) => { };

            Action policy = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_action_with_cancellation_is_null_with_onFallback_with_context()
        {
            Action<CancellationToken> fallbackAction = null;
            Action<Exception, Context> onFallback = (_, __) => { };

            Action policy = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null()
        {
            Action fallbackAction = () => { };
            Action<Exception> onFallback = null;

            Action policy = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onFallback");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null_with_action_with_cancellation()
        {
            Action<CancellationToken> fallbackAction = ct => { };
            Action<Exception> onFallback = null;

            Action policy = () => Policy
                .Handle<DivideByZeroException>()
                .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onFallback");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null_with_context()
        {
            Action fallbackAction = () => { };
            Action<Exception, Context> onFallback = null;

            Action policy = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onFallback");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null_with_context_with_action_with_cancellation()
        {
            Action<CancellationToken> fallbackAction = ct => { };
            Action<Exception, Context> onFallback = null;

            Action policy = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onFallback");
        }

        #endregion

        #region Policy operation tests

        [Fact]
        public void Should_not_execute_fallback_when_execute_delegate_does_not_throw()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction);

            fallbackPolicy.Execute(() => { });

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_not_execute_fallback_when_execute_delegate_throws_exception_not_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction);

            fallbackPolicy.Invoking(x => x.RaiseException<ArgumentNullException>()).ShouldThrow<ArgumentNullException>();

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_execute_fallback_when_execute_delegate_throws_exception_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction);

            fallbackPolicy.Invoking(x => x.RaiseException<DivideByZeroException>()).ShouldNotThrow();

            fallbackActionExecuted.Should().BeTrue();
        }


        [Fact]
        public void Should_execute_fallback_when_execute_delegate_throws_one_of_exceptions_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Or<ArgumentException>()
                                    .Fallback(fallbackAction);

            fallbackPolicy.Invoking(x => x.RaiseException<ArgumentException>()).ShouldNotThrow();

            fallbackActionExecuted.Should().BeTrue();
        }


        [Fact]
        public void Should_not_execute_fallback_when_execute_delegate_throws_exception_not_one_of_exceptions_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Or<NullReferenceException>()
                                    .Fallback(fallbackAction);

            fallbackPolicy.Invoking(x => x.RaiseException<ArgumentNullException>()).ShouldThrow<ArgumentNullException>();

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_not_execute_fallback_when_exception_thrown_does_not_match_handling_predicates()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>(e => false)
                                    .Fallback(fallbackAction);

            fallbackPolicy.Invoking(x => x.RaiseException<DivideByZeroException>()).ShouldThrow<DivideByZeroException>();

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_not_execute_fallback_when_exception_thrown_does_not_match_any_of_handling_predicates()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>(e => false)
                                    .Or<ArgumentNullException>(e => false)
                                    .Fallback(fallbackAction);

            fallbackPolicy.Invoking(x => x.RaiseException<DivideByZeroException>()).ShouldThrow<DivideByZeroException>();

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_execute_fallback_when_exception_thrown_matches_handling_predicates()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>(e => true)
                                    .Fallback(fallbackAction);

            fallbackPolicy.Invoking(x => x.RaiseException<DivideByZeroException>()).ShouldNotThrow();

            fallbackActionExecuted.Should().BeTrue();
        }


        [Fact]
        public void Should_execute_fallback_when_exception_thrown_matches_one_of_handling_predicates()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            FallbackPolicy fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>(e => true)
                                    .Or<ArgumentNullException>()
                                    .Fallback(fallbackAction);

            fallbackPolicy.Invoking(x => x.RaiseException<DivideByZeroException>()).ShouldNotThrow();

            fallbackActionExecuted.Should().BeTrue();
        }

        [Fact]
        public void Should_not_handle_exception_thrown_by_fallback_delegate_even_if_is_exception_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () =>
            {
                fallbackActionExecuted = true; 
                throw new DivideByZeroException() {HelpLink = "FromFallbackAction"};
            };

            FallbackPolicy fallbackPolicy = Policy
                .Handle<DivideByZeroException>()
                .Fallback(fallbackAction);

            fallbackPolicy.Invoking(x => x.RaiseException<DivideByZeroException>((e, i) => e.HelpLink = "FromExecuteDelegate"))
                .ShouldThrow<DivideByZeroException>().And.HelpLink.Should().Be("FromFallbackAction");

            fallbackActionExecuted.Should().BeTrue();
        }

        #endregion

        #region onPolicyEvent delegate tests

        [Fact]
        public void Should_call_onFallback_passing_exception_triggering_fallback()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            Exception exceptionPassedToOnFallback = null;
            Action<Exception> onFallback = ex => { exceptionPassedToOnFallback = ex; };

            FallbackPolicy fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .Fallback(fallbackAction, onFallback);

            Exception instanceToThrow = new ArgumentNullException("myParam");
            fallbackPolicy.Execute(() => { throw instanceToThrow; });

            fallbackActionExecuted.Should().BeTrue();
            exceptionPassedToOnFallback.Should().BeOfType<ArgumentNullException>();
            exceptionPassedToOnFallback.Should().Be(instanceToThrow);
        }

        [Fact]
        public void Should_not_call_onFallback_when_execute_delegate_does_not_throw()
        {
            Action fallbackAction = () => { };

            bool onFallbackExecuted = false;
            Action<Exception> onFallback = _ => { onFallbackExecuted = true; };

            FallbackPolicy fallbackPolicy = Policy
                .Handle<DivideByZeroException>()
                .Fallback(fallbackAction, onFallback);

            fallbackPolicy.Execute(() => { });

            onFallbackExecuted.Should().BeFalse();
        }

        #endregion

        #region Context passing tests

        [Fact]
        public void Should_call_onFallback_with_the_passed_context()
        {
            Action fallbackAction = () => { };

            IDictionary<string, object> contextData = null;

            Action<Exception, Context> onFallback = (ex, ctx) => { contextData = ctx; };

            FallbackPolicy fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .Fallback(fallbackAction, onFallback);

            fallbackPolicy.Invoking(p => p.Execute(() => { throw new ArgumentNullException(); },
                new {key1 = "value1", key2 = "value2"}.AsDictionary()))
                .ShouldNotThrow();

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Should_call_onFallback_with_the_passed_context_when_execute_and_capture()
        {
            Action fallbackAction = () => { };

            IDictionary<string, object> contextData = null;

            Action<Exception, Context> onFallback = (ex, ctx) => { contextData = ctx; };

            FallbackPolicy fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .Fallback(fallbackAction, onFallback);

            fallbackPolicy.Invoking(p => p.ExecuteAndCapture(() => { throw new ArgumentNullException(); },
                new {key1 = "value1", key2 = "value2"}.AsDictionary()))
                .ShouldNotThrow();

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Should_call_onFallback_with_independent_context_for_independent_calls()
        {
            Action fallbackAction = () => { };

            IDictionary<Type, object> contextData = new Dictionary<Type, object>();

            Action<Exception, Context> onFallback = (ex, ctx) => { contextData[ex.GetType()] = ctx["key"]; };

            FallbackPolicy fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .Or<DivideByZeroException>()
                .Fallback(fallbackAction, onFallback);

            fallbackPolicy.Invoking(
                p => p.Execute(() => { throw new ArgumentNullException(); }, new {key = "value1"}.AsDictionary()))
                .ShouldNotThrow();

            fallbackPolicy.Invoking(
                p => p.Execute(() => { throw new DivideByZeroException(); }, new {key = "value2"}.AsDictionary()))
                .ShouldNotThrow();

            contextData.Count.Should().Be(2);
            contextData.Keys.Should().Contain(typeof (ArgumentNullException));
            contextData.Keys.Should().Contain(typeof (DivideByZeroException));
            contextData[typeof (ArgumentNullException)].Should().Be("value1");
            contextData[typeof (DivideByZeroException)].Should().Be("value2");

        }

        [Fact]
        public void Context_should_be_empty_if_execute_not_called_with_any_context_data()
        {
            Context capturedContext = null;
            bool onFallbackExecuted = false;

            Action fallbackAction = () => { };
            Action<Exception, Context> onFallback = (ex, ctx) => { onFallbackExecuted = true; capturedContext = ctx; };

            FallbackPolicy fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .Or<DivideByZeroException>()
                .Fallback(fallbackAction, onFallback);

            fallbackPolicy.RaiseException<DivideByZeroException>();

            onFallbackExecuted.Should().BeTrue();
            capturedContext.Should().BeEmpty();
        }

        #endregion


        #region Cancellation tests

        [Fact]
        public void Should_execute_action_when_non_faulting_and_cancellationtoken_not_cancelled()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            FallbackPolicy policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null,
            };

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldNotThrow();
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_execute_fallback_when_faulting_and_cancellationtoken_not_cancelled()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            FallbackPolicy policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = null,
            };

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldNotThrow();
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeTrue();
        }

        [Fact]
        public void Should_not_execute_action_when_cancellationtoken_cancelled_before_execute()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            FallbackPolicy policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction);

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

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
            attemptsInvoked.Should().Be(0);

            fallbackActionExecuted.Should().BeFalse();

        }

        [Fact]
        public void Should_report_cancellation_and_not_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationtoken()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            FallbackPolicy policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction);

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

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_report_cancellation_and_not_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_does_not_observe_cancellationtoken()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            FallbackPolicy policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction);

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

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_report_cancellation_after_faulting_action_execution_and_not_call_fallback_function_if_onFallback_invokes_cancellation()
        {
            bool fallbackActionExecuted = false;
            Action fallbackAction = () => { fallbackActionExecuted = true; };

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Action<Exception> onFallback = _ => { cancellationTokenSource.Cancel(); };

            FallbackPolicy policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Fallback(fallbackAction, onFallback);

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = null, // Cancellation during onFallback instead - see above.
                ActionObservesCancellation = false
            };

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeFalse();
        }
        
        #endregion



    }
}
