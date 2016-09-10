﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Fallback;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs
{
    public class FallbackTResultAsyncSpecs
    {
        #region Configuration guard condition tests

        [Fact]
        public void Should_throw_when_fallback_action_is_null()
        {
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = null;

            Action policy = () => Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .FallbackAsync(fallbackAction);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_action_is_null_with_onFallback()
        {
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = null;
            Func<DelegateResult<ResultPrimitive>, Task> onFallbackAsync = _ => TaskHelper.EmptyTask;

            Action policy = () => Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .FallbackAsync(fallbackAction, onFallbackAsync);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_action_is_null_with_onFallback_with_context()
        {
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = null;
            Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (_, __) => TaskHelper.EmptyTask;

            Action policy = () => Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .FallbackAsync(fallbackAction, onFallbackAsync);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null()
        {
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => Task.FromResult(ResultPrimitive.Substitute);
            Func<DelegateResult<ResultPrimitive>, Task> onFallbackAsync = null;

            Action policy = () => Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .FallbackAsync(fallbackAction, onFallbackAsync);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onFallbackAsync");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null_with_action_with_cancellation()
        {
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => Task.FromResult(ResultPrimitive.Substitute);
            Func<DelegateResult<ResultPrimitive>, Task> onFallbackAsync = null;

            Action policy = () => Policy
                .HandleResult(ResultPrimitive.Fault)
                .FallbackAsync(fallbackAction, onFallbackAsync);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onFallbackAsync");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null_with_context()
        {
                        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => Task.FromResult(ResultPrimitive.Substitute);
            Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = null;

            Action policy = () => Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .FallbackAsync(fallbackAction, onFallbackAsync);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onFallbackAsync");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null_with_context_with_action_with_cancellation()
        {
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => Task.FromResult(ResultPrimitive.Substitute);
            Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = null;

            Action policy = () => Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .FallbackAsync(fallbackAction, onFallbackAsync);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onFallbackAsync");
        }

        #endregion

        #region Policy operation tests

        [Fact]
        public void Should_not_execute_fallback_when_execute_delegate_does_not_raise_fault()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .FallbackAsync(fallbackAction);

            fallbackPolicy.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good));

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public async void Should_not_execute_fallback_when_execute_delegate_raises_fault_not_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .FallbackAsync(fallbackAction);

            (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.FaultAgain);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public async void Should_return_fallback_value_when_execute_delegate_raises_fault_handled_by_policy()
        {
            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .FallbackAsync(ResultPrimitive.Substitute);

            (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Substitute);
        }

        [Fact]
        public async void Should_execute_fallback_when_execute_delegate_raises_fault_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .FallbackAsync(fallbackAction);

            (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Substitute);

            fallbackActionExecuted.Should().BeTrue();
        }

        [Fact]
        public async void Should_execute_fallback_when_execute_delegate_raises_one_of_results_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .OrResult(ResultPrimitive.FaultAgain)
                                    .FallbackAsync(fallbackAction);

            (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Substitute);

            fallbackActionExecuted.Should().BeTrue();
        }

        [Fact]
        public async void Should_not_execute_fallback_when_execute_delegate_raises_fault_not_one_of_faults_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .OrResult(ResultPrimitive.FaultAgain)
                                    .FallbackAsync(fallbackAction);

            (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.FaultYetAgain).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.FaultYetAgain);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public async void Should_not_execute_fallback_when_result_raised_does_not_match_handling_predicates()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult<ResultPrimitive>(r => false)
                                    .FallbackAsync(fallbackAction);

            (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public async void Should_not_execute_fallback_when_execute_delegate_raises_fault_not_handled_by_any_of_predicates()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult<ResultPrimitive>(r => r == ResultPrimitive.Fault)
                                    .OrResult(r => r == ResultPrimitive.FaultAgain)
                                    .FallbackAsync(fallbackAction);

            (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.FaultYetAgain).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.FaultYetAgain);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public async void Should_execute_fallback_when_result_raised_matches_handling_predicates()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult<ResultPrimitive>(r => true)
                                    .FallbackAsync(fallbackAction);

            (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.Undefined).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Substitute);

            fallbackActionExecuted.Should().BeTrue();
        }


        [Fact]
        public async void Should_execute_fallback_when_result_raised_matches_one_of_handling_predicates()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult<ResultPrimitive>(r => true)
                                    .OrResult(ResultPrimitive.FaultAgain)
                                    .FallbackAsync(fallbackAction);

            (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.Undefined).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Substitute);

            fallbackActionExecuted.Should().BeTrue();
        }

        [Fact]
        public async void Should_not_handle_result_raised_by_fallback_delegate_even_if_is_result_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task<ResultClass>> fallbackAction = ct =>
            {
                fallbackActionExecuted = true;
                return Task.FromResult(new ResultClass(ResultPrimitive.Fault, "FromFallbackAction"));
            };

            FallbackPolicy<ResultClass> fallbackPolicy = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .FallbackAsync(fallbackAction);

            (await fallbackPolicy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.Fault, "FromExecuteDelegate")).ConfigureAwait(false))
                .Should().Match<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault && r.SomeString == "FromFallbackAction");

            fallbackActionExecuted.Should().BeTrue();
        }

        #endregion

        #region onPolicyEvent delegate tests

        [Fact]
        public void Should_call_onFallback_passing_result_triggering_fallback()
        {
            bool fallbackActionExecuted = false;
            Func<CancellationToken, Task<ResultClass>> fallbackAction = ct => { fallbackActionExecuted = true; return Task.FromResult(new ResultClass(ResultPrimitive.Substitute)); };

            ResultClass resultPassedToOnFallback = null;
            Func<DelegateResult<ResultClass>, Task> onFallbackAsync = r => { resultPassedToOnFallback = r.Result; return TaskHelper.EmptyTask; };

            FallbackPolicy<ResultClass> fallbackPolicy = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .FallbackAsync(fallbackAction, onFallbackAsync);

            ResultClass resultFromDelegate = new ResultClass(ResultPrimitive.Fault);
            fallbackPolicy.ExecuteAsync(() => { return Task.FromResult(resultFromDelegate); });

            fallbackActionExecuted.Should().BeTrue();
            resultPassedToOnFallback.Should().NotBeNull();
            resultPassedToOnFallback.Should().Be(resultFromDelegate);
        }

        [Fact]
        public void Should_not_call_onFallback_when_execute_delegate_does_not_raise_fault()
        {
                        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => Task.FromResult(ResultPrimitive.Substitute);

            bool onFallbackExecuted = false;
            Func<DelegateResult<ResultPrimitive>, Task> onFallbackAsync = _ => { onFallbackExecuted = true; return TaskHelper.EmptyTask; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .FallbackAsync(fallbackAction, onFallbackAsync);

            fallbackPolicy.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good));

            onFallbackExecuted.Should().BeFalse();
        }

        #endregion

        #region Context passing tests

        [Fact]
        public void Should_call_onFallback_with_the_passed_context()
        {
                        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => Task.FromResult(ResultPrimitive.Substitute);

            IDictionary<string, object> contextData = null;

            Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (dr, ctx) => { contextData = ctx; return TaskHelper.EmptyTask; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .FallbackAsync(fallbackAction, onFallbackAsync);

            fallbackPolicy.ExecuteAsync(() => { return Task.FromResult(ResultPrimitive.Fault); },
                new { key1 = "value1", key2 = "value2" }.AsDictionary())
                .Result
                .Should().Be(ResultPrimitive.Substitute);

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public async void Should_call_onFallback_with_the_passed_context_when_execute_and_capture()
        {
                        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => Task.FromResult(ResultPrimitive.Substitute);

            IDictionary<string, object> contextData = null;

            Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (ex, ctx) => { contextData = ctx; return TaskHelper.EmptyTask; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .FallbackAsync(fallbackAction, onFallbackAsync);

            (await fallbackPolicy.ExecuteAndCaptureAsync(() => { return Task.FromResult(ResultPrimitive.Fault); },
                new { key1 = "value1", key2 = "value2" }.AsDictionary()).ConfigureAwait(false))
                .Result.Should().Be(ResultPrimitive.Substitute);

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Should_call_onFallback_with_independent_context_for_independent_calls()
        {
                        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => Task.FromResult(ResultPrimitive.Substitute);

            IDictionary<ResultPrimitive, object> contextData = new Dictionary<ResultPrimitive, object>();

            Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (dr, ctx) => { contextData[dr.Result] = ctx["key"]; return TaskHelper.EmptyTask; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .FallbackAsync(fallbackAction, onFallbackAsync);

            fallbackPolicy.ExecuteAsync(() => { return Task.FromResult(ResultPrimitive.Fault); }, new { key = "value1" }.AsDictionary())
                .Result
                .Should().Be(ResultPrimitive.Substitute);

            fallbackPolicy.ExecuteAsync(() => { return Task.FromResult(ResultPrimitive.FaultAgain); }, new { key = "value2" }.AsDictionary())
                .Result
                .Should().Be(ResultPrimitive.Substitute);

            contextData.Count.Should().Be(2);
            contextData.Keys.Should().Contain(ResultPrimitive.Fault);
            contextData.Keys.Should().Contain(ResultPrimitive.FaultAgain);
            contextData[ResultPrimitive.Fault].Should().Be("value1");
            contextData[ResultPrimitive.FaultAgain].Should().Be("value2");
        }

        [Fact]
        public async void Context_should_be_empty_if_execute_not_called_with_any_context_data()
        {
            Context capturedContext = null;
            bool onFallbackExecuted = false;

                        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = ct => Task.FromResult(ResultPrimitive.Substitute);
            Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (ex, ctx) => { onFallbackExecuted = true; capturedContext = ctx; return TaskHelper.EmptyTask; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .FallbackAsync(fallbackAction, onFallbackAsync);

            (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Substitute);

            onFallbackExecuted.Should().BeTrue();
            capturedContext.Should().BeEmpty();
        }

        #endregion

    }
}
