using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Specs.Helpers;
using Xunit;

using Scenario = Polly.Specs.Helpers.PolicyExtensionsAsync.ExceptionAndOrCancellationScenario;

namespace Polly.Specs.Fallback
{
    public class FallbackAsyncSpecs
    {
        #region Configuration guard condition tests

        [Fact]
        public void Should_throw_when_fallback_func_is_null()
        {
            Func<CancellationToken, Task> fallbackActionAsync = null;

            Action policy = () => Policy
                .Handle<DivideByZeroException>()
                .FallbackAsync(fallbackActionAsync);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_func_is_null_with_onFallback()
        {
            Func<CancellationToken, Task> fallbackActionAsync = null;
            Func<Exception, Task> onFallbackAsync = _ => Task.CompletedTask;

            Action policy = () => Policy
                .Handle<DivideByZeroException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_func_is_null_with_onFallback_with_context()
        {
            Func<Context, CancellationToken, Task> fallbackActionAsync = null;
            Func<Exception, Context, Task> onFallbackAsync = (_, __) => Task.CompletedTask;

            Action policy = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null()
        {
            Func<CancellationToken, Task> fallbackActionAsync = ct => Task.CompletedTask;
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
            Func<Context, CancellationToken, Task> fallbackActionAsync = (_, __) => Task.CompletedTask;
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
        public async Task Should_not_execute_fallback_when_executed_delegate_does_not_throw()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            await fallbackPolicy.ExecuteAsync(() => Task.CompletedTask);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_not_execute_fallback_when_executed_delegate_throws_exception_not_handled_by_policy()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(async x => await x.RaiseExceptionAsync<ArgumentNullException>()).ShouldThrow<ArgumentNullException>();

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_execute_fallback_when_executed_delegate_throws_exception_handled_by_policy()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>()).ShouldNotThrow();

            fallbackActionExecuted.Should().BeTrue();
        }


        [Fact]
        public void Should_execute_fallback_when_executed_delegate_throws_one_of_exceptions_handled_by_policy()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Or<ArgumentException>()
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(async x => await x.RaiseExceptionAsync<ArgumentException>()).ShouldNotThrow();

            fallbackActionExecuted.Should().BeTrue();
        }


        [Fact]
        public void Should_not_execute_fallback_when_executed_delegate_throws_exception_not_one_of_exceptions_handled_by_policy()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Or<NullReferenceException>()
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(async x => await x.RaiseExceptionAsync<ArgumentNullException>()).ShouldThrow<ArgumentNullException>();

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_not_execute_fallback_when_exception_thrown_does_not_match_handling_predicates()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>(e => false)
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>()).ShouldThrow<DivideByZeroException>();

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_not_execute_fallback_when_exception_thrown_does_not_match_any_of_handling_predicates()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>(e => false)
                                    .Or<ArgumentNullException>(e => false)
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>()).ShouldThrow<DivideByZeroException>();

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_execute_fallback_when_exception_thrown_matches_handling_predicates()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>(e => true)
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>()).ShouldNotThrow();

            fallbackActionExecuted.Should().BeTrue();
        }


        [Fact]
        public void Should_execute_fallback_when_exception_thrown_matches_one_of_handling_predicates()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                                    .Handle<DivideByZeroException>(e => true)
                                    .Or<ArgumentNullException>()
                                    .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>()).ShouldNotThrow();

            fallbackActionExecuted.Should().BeTrue();
        }

        [Fact]
        public void Should_not_handle_exception_thrown_by_fallback_delegate_even_if_is_exception_handled_by_policy()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ =>
           {
               fallbackActionExecuted = true;
               throw new DivideByZeroException() { HelpLink = "FromFallbackAction" };
           };

            var fallbackPolicy = Policy
                .Handle<DivideByZeroException>()
                .FallbackAsync(fallbackActionAsync);

            fallbackPolicy.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>((e, i) => e.HelpLink = "FromExecuteDelegate"))
                .ShouldThrow<DivideByZeroException>().And.HelpLink.Should().Be("FromFallbackAction");

            fallbackActionExecuted.Should().BeTrue();
        }

        [Fact]
        public void Should_throw_for_generic_method_execution_on_non_generic_policy()
        {
            var fallbackPolicy = Policy
                .Handle<DivideByZeroException>()
                .FallbackAsync(_ => Task.CompletedTask);

            fallbackPolicy.Awaiting(p => p.ExecuteAsync<int>(() => Task.FromResult(0))).ShouldThrow<InvalidOperationException>();
        }

        #endregion

        #region onPolicyEvent delegate tests

        [Fact]
        public async Task Should_call_onFallback_passing_exception_triggering_fallback()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            Exception exceptionPassedToOnFallback = null;
            Func<Exception, Task> onFallbackAsync = ex => { exceptionPassedToOnFallback = ex; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            Exception instanceToThrow = new ArgumentNullException("myParam");
            await fallbackPolicy.RaiseExceptionAsync(instanceToThrow);

            fallbackActionExecuted.Should().BeTrue();
            exceptionPassedToOnFallback.Should().BeOfType<ArgumentNullException>();
            exceptionPassedToOnFallback.Should().Be(instanceToThrow);
        }

        [Fact]
        public async Task Should_not_call_onFallback_when_executed_delegate_does_not_throw()
        {
            Func<CancellationToken, Task> fallbackActionAsync = _ => Task.CompletedTask;

            var onFallbackExecuted = false;
            Func<Exception, Task> onFallbackAsync = ex => { onFallbackExecuted = true; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                .Handle<DivideByZeroException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            await fallbackPolicy.ExecuteAsync(() => Task.CompletedTask);

            onFallbackExecuted.Should().BeFalse();
        }

        #endregion

        #region Context passing tests

        [Fact]
        public void Should_call_onFallback_with_the_passed_context()
        {
            Func<Context, CancellationToken, Task> fallbackActionAsync = (_, __) => Task.CompletedTask;

            IDictionary<string, object> contextData = null;

            Func<Exception, Context, Task> onFallbackAsync = (ex, ctx) => { contextData = ctx; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            fallbackPolicy.Awaiting(async p => await p.ExecuteAsync(ctx => { throw new ArgumentNullException(); },
                new { key1 = "value1", key2 = "value2" }.AsDictionary()))
                .ShouldNotThrow();

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Should_call_onFallback_with_the_passed_context_when_execute_and_capture()
        {
            Func<Context, CancellationToken, Task> fallbackActionAsync = (_, __) => Task.CompletedTask;

            IDictionary<string, object> contextData = null;

            Func<Exception, Context, Task> onFallbackAsync = (ex, ctx) => { contextData = ctx; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            fallbackPolicy.Awaiting(async p => await p.ExecuteAndCaptureAsync(ctx => { throw new ArgumentNullException(); },
                new { key1 = "value1", key2 = "value2" }.AsDictionary()))
                .ShouldNotThrow();

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Should_call_onFallback_with_independent_context_for_independent_calls()
        {
            Func<Context, CancellationToken, Task> fallbackActionAsync = (_, __) => Task.CompletedTask;

            IDictionary<Type, object> contextData = new Dictionary<Type, object>();

            Func<Exception, Context, Task> onFallbackAsync = (ex, ctx) => { contextData[ex.GetType()] = ctx["key"]; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .Or<DivideByZeroException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            fallbackPolicy.Awaiting(async
                p => await p.ExecuteAsync(ctx => { throw new ArgumentNullException(); }, new { key = "value1" }.AsDictionary()))
                .ShouldNotThrow();

            fallbackPolicy.Awaiting(async
                p => await p.ExecuteAsync(ctx => { throw new DivideByZeroException(); }, new { key = "value2" }.AsDictionary()))
                .ShouldNotThrow();

            contextData.Count.Should().Be(2);
            contextData.Keys.Should().Contain(typeof(ArgumentNullException));
            contextData.Keys.Should().Contain(typeof(DivideByZeroException));
            contextData[typeof(ArgumentNullException)].Should().Be("value1");
            contextData[typeof(DivideByZeroException)].Should().Be("value2");

        }

        [Fact]
        public async Task Context_should_be_empty_if_execute_not_called_with_any_context_data()
        {
            Context capturedContext = null;
            var onFallbackExecuted = false;

            Func<Context, CancellationToken, Task> fallbackActionAsync = (_, __) => Task.CompletedTask;
            Func<Exception, Context, Task> onFallbackAsync = (ex, ctx) => { onFallbackExecuted = true; capturedContext = ctx; return Task.CompletedTask; };

            var fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .Or<DivideByZeroException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            await fallbackPolicy.RaiseExceptionAsync<DivideByZeroException>();

            onFallbackExecuted.Should().BeTrue();
            capturedContext.Should().BeEmpty();
        }

        [Fact]
        public void Should_call_fallbackAction_with_the_passed_context()
        {
            IDictionary<string, object> contextData = null;

            Func<Context, CancellationToken, Task> fallbackActionAsync = (ctx, ct) => { contextData = ctx; return Task.CompletedTask; };

            Func<Exception, Context, Task> onFallbackAsync = (ex, ctx) => Task.CompletedTask;

            var fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            fallbackPolicy.Awaiting(async p => await p.ExecuteAsync(ctx => { throw new ArgumentNullException(); },
                    new { key1 = "value1", key2 = "value2" }.AsDictionary()))
                .ShouldNotThrow();

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Should_call_fallbackAction_with_the_passed_context_when_execute_and_capture()
        {
            IDictionary<string, object> contextData = null;

            Func<Context, CancellationToken, Task> fallbackActionAsync = (ctx, ct) => { contextData = ctx; return Task.CompletedTask; };

            Func<Exception, Context, Task> onFallbackAsync = (ex, ctx) => Task.CompletedTask;

            var fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            fallbackPolicy.Awaiting(async p => await p.ExecuteAndCaptureAsync(ctx => { throw new ArgumentNullException(); },
                    new { key1 = "value1", key2 = "value2" }.AsDictionary()))
                .ShouldNotThrow();

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public async Task Context_should_be_empty_at_fallbackAction_if_execute_not_called_with_any_context_data()
        {
            Context capturedContext = null;
            var fallbackExecuted = false;

            Func<Context, CancellationToken, Task> fallbackActionAsync = (ctx, ct) => { fallbackExecuted = true; capturedContext = ctx; return Task.CompletedTask; };

            Func<Exception, Context, Task> onFallbackAsync = (ex, ctx) => Task.CompletedTask;

            var fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .Or<DivideByZeroException>()
                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

            await fallbackPolicy.RaiseExceptionAsync<DivideByZeroException>();

            fallbackExecuted.Should().BeTrue();
            capturedContext.Should().BeEmpty();
        }

        #endregion

        #region Exception passing tests

        [Fact]
        public void Should_call_fallbackAction_with_the_exception()
        {
            Exception fallbackException = null;

            Func<Exception, Context, CancellationToken, Task> fallbackFunc = (ex, ctx, ct) => { fallbackException = ex; return Task.CompletedTask; };

            Func<Exception, Context, Task> onFallback = (ex, ctx) => { return Task.CompletedTask; };

            var fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .FallbackAsync(fallbackFunc, onFallback);

            Exception instanceToThrow = new ArgumentNullException("myParam");
            fallbackPolicy.Awaiting(p => p.RaiseExceptionAsync(instanceToThrow))
                .ShouldNotThrow();

            fallbackException.Should().Be(instanceToThrow);
        }

        [Fact]
        public void Should_call_fallbackAction_with_the_exception_when_execute_and_capture()
        {
            Exception fallbackException = null;

            Func<Exception, Context, CancellationToken, Task> fallbackFunc = (ex, ctx, ct) => { fallbackException = ex; return Task.CompletedTask; };

            Func<Exception, Context, Task> onFallback = (ex, ctx) => { return Task.CompletedTask; };

            var fallbackPolicy = Policy
                .Handle<ArgumentNullException>()
                .FallbackAsync(fallbackFunc, onFallback);

            fallbackPolicy.Awaiting(async p => await p.ExecuteAndCaptureAsync(() => { throw new ArgumentNullException(); }))
                .ShouldNotThrow();

            fallbackException.Should().NotBeNull()
                .And.BeOfType(typeof(ArgumentNullException));
        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Should_call_fallbackAction_with_the_matched_inner_exception_unwrapped()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            Exception fallbackException = null;

            Func<Exception, Context, CancellationToken, Task> fallbackFunc = (ex, ctx, ct) => { fallbackException = ex; return Task.CompletedTask; };

            Func<Exception, Context, Task> onFallback = (ex, ctx) => { return Task.CompletedTask; };

            var fallbackPolicy = Policy
                .HandleInner<ArgumentNullException>()
                .FallbackAsync(fallbackFunc, onFallback);

            Exception instanceToCapture = new ArgumentNullException("myParam");
            var instanceToThrow = new Exception(string.Empty, instanceToCapture);
            fallbackPolicy.Awaiting(p => p.RaiseExceptionAsync(instanceToThrow))
                .ShouldNotThrow();

            fallbackException.Should().Be(instanceToCapture);
        }

        [Fact]
        public void Should_call_fallbackAction_with_the_matched_inner_of_aggregate_exception_unwrapped()
        {
            Exception fallbackException = null;

            Func<Exception, Context, CancellationToken, Task> fallbackFunc = (ex, ctx, ct) => { fallbackException = ex; return Task.CompletedTask; };

            Func<Exception, Context, Task> onFallback = (ex, ctx) => { return Task.CompletedTask; };

            var fallbackPolicy = Policy
                .HandleInner<ArgumentNullException>()
                .FallbackAsync(fallbackFunc, onFallback);

            Exception instanceToCapture = new ArgumentNullException("myParam");
            Exception instanceToThrow = new AggregateException(instanceToCapture);
            fallbackPolicy.Awaiting(p => p.RaiseExceptionAsync(instanceToThrow))
                .ShouldNotThrow();

            fallbackException.Should().Be(instanceToCapture);
        }

        [Fact]
        public void Should_not_call_fallbackAction_with_the_exception_if_exception_unhandled()
        {
            Exception fallbackException = null;

            Func<Exception, Context, CancellationToken, Task> fallbackFunc = (ex, ctx, ct) => { fallbackException = ex; return Task.CompletedTask; };

            Func<Exception, Context, Task> onFallback = (ex, ctx) => { return Task.CompletedTask; };

            var fallbackPolicy = Policy
                .Handle<DivideByZeroException>()
                .FallbackAsync(fallbackFunc, onFallback);

            fallbackPolicy.Awaiting(async p => await p.ExecuteAsync(() => { throw new ArgumentNullException(); }))
                .ShouldThrow<ArgumentNullException>();

            fallbackException.Should().BeNull();
        }

        #endregion

        #region Cancellation tests

        [Fact]
        public void Should_execute_action_when_non_faulting_and_cancellationtoken_not_cancelled()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            var scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null,
            };

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldNotThrow();
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_execute_fallback_when_faulting_and_cancellationtoken_not_cancelled()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            var scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = null,
            };

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldNotThrow();
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeTrue();
        }

        [Fact]
        public void Should_not_execute_action_when_cancellationtoken_cancelled_before_execute()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            var scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
            };

            cancellationTokenSource.Cancel();

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
            attemptsInvoked.Should().Be(0);

            fallbackActionExecuted.Should().BeFalse();

        }

        [Fact]
        public void Should_report_cancellation_and_not_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationtoken_and_fallback_does_not_handle_cancellations()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            var scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_handle_cancellation_and_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationtoken_and_fallback_handles_cancellations()
        {
            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .Or<OperationCanceledException>()
                                    .FallbackAsync(fallbackActionAsync);

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            var scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldNotThrow();
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeTrue();
        }

        [Fact]
        public void Should_not_report_cancellation_and_not_execute_fallback_if_non_faulting_action_execution_completes_and_user_delegate_does_not_observe_the_set_cancellationtoken()
        {

            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };

            var policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            var scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = false
            };

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldNotThrow();
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_report_unhandled_fault_and_not_execute_fallback_if_action_execution_raises_unhandled_fault_and_user_delegate_does_not_observe_the_set_cancellationtoken()
        {

            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };


            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            var attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            var scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = false
            };

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<NullReferenceException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<NullReferenceException>();
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_handle_handled_fault_and_execute_fallback_following_faulting_action_execution_when_user_delegate_does_not_observe_cancellationtoken()
        {

            var fallbackActionExecuted = false;
            Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return Task.CompletedTask; };


            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var policy = Policy
                                    .Handle<DivideByZeroException>()
                                    .FallbackAsync(fallbackActionAsync);

            var attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            var scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = false
            };

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldNotThrow();
            attemptsInvoked.Should().Be(1);

            fallbackActionExecuted.Should().BeTrue();
        }

        #endregion


    }
}
