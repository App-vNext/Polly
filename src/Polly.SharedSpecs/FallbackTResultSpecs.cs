using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Polly.Fallback;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs
{
    public class FallbackTResultSpecs
    {
        #region Configuration guard condition tests

        [Fact]
        public void Should_throw_when_fallback_action_is_null()
        {
            Func<ResultPrimitive> fallbackAction = null;

            Action policy = () => Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .Fallback(fallbackAction);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_action_with_cancellation_is_null()
        {
            Func<CancellationToken, ResultPrimitive> fallbackAction = null;

            Action policy = () => Policy
                .HandleResult(ResultPrimitive.Fault)
                .Fallback(fallbackAction);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_action_is_null_with_onFallback()
        {
            Func<ResultPrimitive> fallbackAction = null;
            Action<DelegateResult<ResultPrimitive>> onFallback = _ => { };

            Action policy = () => Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_action_with_cancellation_is_null_with_onFallback()
        {
            Func<CancellationToken, ResultPrimitive> fallbackAction = null;
            Action<DelegateResult<ResultPrimitive>> onFallback = _ => { };

            Action policy = () => Policy
                .HandleResult(ResultPrimitive.Fault)
                .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_action_is_null_with_onFallback_with_context()
        {
            Func<ResultPrimitive> fallbackAction = null;
            Action<DelegateResult<ResultPrimitive>, Context> onFallback = (_, __) => { };

            Action policy = () => Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_fallback_action_with_cancellation_is_null_with_onFallback_with_context()
        {
            Func<CancellationToken, ResultPrimitive> fallbackAction = null;
            Action<DelegateResult<ResultPrimitive>, Context> onFallback = (_, __) => { };

            Action policy = () => Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("fallbackAction");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null()
        {
            Func<ResultPrimitive> fallbackAction = () => ResultPrimitive.Substitute;
            Action<DelegateResult<ResultPrimitive>> onFallback = null;

            Action policy = () => Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onFallback");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null_with_action_with_cancellation()
        {
            Func<CancellationToken, ResultPrimitive> fallbackAction = ct => ResultPrimitive.Substitute;
            Action<DelegateResult<ResultPrimitive>> onFallback = null;

            Action policy = () => Policy
                .HandleResult(ResultPrimitive.Fault)
                .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onFallback");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null_with_context()
        {
            Func<ResultPrimitive> fallbackAction = () => ResultPrimitive.Substitute;
            Action<DelegateResult<ResultPrimitive>, Context> onFallback = null;

            Action policy = () => Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onFallback");
        }

        [Fact]
        public void Should_throw_when_onFallback_delegate_is_null_with_context_with_action_with_cancellation()
        {
            Func<CancellationToken, ResultPrimitive> fallbackAction = ct => ResultPrimitive.Substitute;
            Action<DelegateResult<ResultPrimitive>, Context> onFallback = null;

            Action policy = () => Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .Fallback(fallbackAction, onFallback);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onFallback");
        }

        #endregion

        #region Policy operation tests

        [Fact]
        public void Should_not_execute_fallback_when_execute_delegate_does_not_raise_fault()
        {
            bool fallbackActionExecuted = false;
            Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .Fallback(fallbackAction);

            fallbackPolicy.Execute(() => ResultPrimitive.Good);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_not_execute_fallback_when_execute_delegate_raises_fault_not_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .Fallback(fallbackAction);

            fallbackPolicy.RaiseResultSequence(ResultPrimitive.FaultAgain).Should().Be(ResultPrimitive.FaultAgain);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_return_fallback_value_when_execute_delegate_raises_fault_handled_by_policy()
        {
            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .Fallback(ResultPrimitive.Substitute);

            fallbackPolicy.RaiseResultSequence(ResultPrimitive.Fault).Should().Be(ResultPrimitive.Substitute);
        }

        [Fact]
        public void Should_execute_fallback_when_execute_delegate_raises_fault_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .Fallback(fallbackAction);

            fallbackPolicy.RaiseResultSequence(ResultPrimitive.Fault).Should().Be(ResultPrimitive.Substitute);

            fallbackActionExecuted.Should().BeTrue();
        }
        
        [Fact]
        public void Should_execute_fallback_when_execute_delegate_raises_one_of_results_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .OrResult(ResultPrimitive.FaultAgain)
                                    .Fallback(fallbackAction);

            fallbackPolicy.RaiseResultSequence(ResultPrimitive.FaultAgain).Should().Be(ResultPrimitive.Substitute);

            fallbackActionExecuted.Should().BeTrue();
        }

        [Fact]
        public void Should_not_execute_fallback_when_execute_delegate_raises_fault_not_one_of_faults_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult(ResultPrimitive.Fault)
                                    .OrResult(ResultPrimitive.FaultAgain)
                                    .Fallback(fallbackAction);

            fallbackPolicy.RaiseResultSequence(ResultPrimitive.FaultYetAgain).Should().Be(ResultPrimitive.FaultYetAgain);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_not_execute_fallback_when_result_raised_does_not_match_handling_predicates()
        {
            bool fallbackActionExecuted = false;
            Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult<ResultPrimitive>(r => false)
                                    .Fallback(fallbackAction);

            fallbackPolicy.RaiseResultSequence(ResultPrimitive.Fault).Should().Be(ResultPrimitive.Fault);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_not_execute_fallback_when_execute_delegate_raises_fault_not_handled_by_any_of_predicates()
        {
            bool fallbackActionExecuted = false;
            Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult<ResultPrimitive>(r => r == ResultPrimitive.Fault)
                                    .OrResult(r => r == ResultPrimitive.FaultAgain)
                                    .Fallback(fallbackAction);

            fallbackPolicy.RaiseResultSequence(ResultPrimitive.FaultYetAgain).Should().Be(ResultPrimitive.FaultYetAgain);

            fallbackActionExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_execute_fallback_when_result_raised_matches_handling_predicates()
        {
            bool fallbackActionExecuted = false;
            Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult<ResultPrimitive>(r => true)
                                    .Fallback(fallbackAction);

            fallbackPolicy.RaiseResultSequence(ResultPrimitive.Undefined).Should().Be(ResultPrimitive.Substitute);

            fallbackActionExecuted.Should().BeTrue();
        }


        [Fact]
        public void Should_execute_fallback_when_result_raised_matches_one_of_handling_predicates()
        {
            bool fallbackActionExecuted = false;
            Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                    .HandleResult<ResultPrimitive>(r => true)
                                    .OrResult(ResultPrimitive.FaultAgain)
                                    .Fallback(fallbackAction);

            fallbackPolicy.RaiseResultSequence(ResultPrimitive.Undefined).Should().Be(ResultPrimitive.Substitute);

            fallbackActionExecuted.Should().BeTrue();
        }

        [Fact]
        public void Should_not_handle_result_raised_by_fallback_delegate_even_if_is_result_handled_by_policy()
        {
            bool fallbackActionExecuted = false;
            Func<ResultClass> fallbackAction = () =>
            {
                fallbackActionExecuted = true;
                return new ResultClass(ResultPrimitive.Fault, "FromFallbackAction");
            };

            FallbackPolicy<ResultClass> fallbackPolicy = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .Fallback(fallbackAction);

            fallbackPolicy.RaiseResultSequence(new ResultClass(ResultPrimitive.Fault, "FromExecuteDelegate"))
                .Should().Match<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault && r.SomeString == "FromFallbackAction");

            fallbackActionExecuted.Should().BeTrue();
        }

        #endregion

        #region onPolicyEvent delegate tests

        [Fact]
        public void Should_call_onFallback_passing_result_triggering_fallback()
        {
            bool fallbackActionExecuted = false;
            Func<ResultClass> fallbackAction = () => { fallbackActionExecuted = true; return new ResultClass(ResultPrimitive.Substitute); };

            ResultClass resultPassedToOnFallback = null;
            Action<DelegateResult<ResultClass>> onFallback = r => { resultPassedToOnFallback = r.Result; };

            FallbackPolicy<ResultClass> fallbackPolicy = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .Fallback(fallbackAction, onFallback);

            ResultClass resultFromDelegate = new ResultClass(ResultPrimitive.Fault);
            fallbackPolicy.Execute(() => { return resultFromDelegate; });

            fallbackActionExecuted.Should().BeTrue();
            resultPassedToOnFallback.Should().NotBeNull();
            resultPassedToOnFallback.Should().Be(resultFromDelegate);
        }

        [Fact]
        public void Should_not_call_onFallback_when_execute_delegate_does_not_raise_fault()
        {
            Func<ResultPrimitive> fallbackAction = () => ResultPrimitive.Substitute;

            bool onFallbackExecuted = false;
            Action<DelegateResult<ResultPrimitive>> onFallback = _ => { onFallbackExecuted = true; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Fallback(fallbackAction, onFallback);

            fallbackPolicy.Execute(() => ResultPrimitive.Good);

            onFallbackExecuted.Should().BeFalse();
        }

        #endregion

        #region Context passing tests

        [Fact]
        public void Should_call_onFallback_with_the_passed_context()
        {
            Func<ResultPrimitive> fallbackAction = () => ResultPrimitive.Substitute;

            IDictionary<string, object> contextData = null;

            Action<DelegateResult<ResultPrimitive>, Context> onFallback = (dr, ctx) => { contextData = ctx; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Fallback(fallbackAction, onFallback);

            fallbackPolicy.Execute(() => { return ResultPrimitive.Fault; },
                new { key1 = "value1", key2 = "value2" }.AsDictionary())
                .Should().Be(ResultPrimitive.Substitute);

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Should_call_onFallback_with_the_passed_context_when_execute_and_capture()
        {
            Func<ResultPrimitive> fallbackAction = () => ResultPrimitive.Substitute;

            IDictionary<string, object> contextData = null;

            Action<DelegateResult<ResultPrimitive>, Context> onFallback = (ex, ctx) => { contextData = ctx; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Fallback(fallbackAction, onFallback);

            fallbackPolicy.ExecuteAndCapture(() => { return ResultPrimitive.Fault; },
                new { key1 = "value1", key2 = "value2" }.AsDictionary())
                .Result.Should().Be(ResultPrimitive.Substitute);

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Should_call_onFallback_with_independent_context_for_independent_calls()
        {
            Func<ResultPrimitive> fallbackAction = () => ResultPrimitive.Substitute;

            IDictionary<ResultPrimitive, object> contextData = new Dictionary<ResultPrimitive, object>();

            Action<DelegateResult<ResultPrimitive>, Context> onFallback = (dr, ctx) => { contextData[dr.Result] = ctx["key"]; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .Fallback(fallbackAction, onFallback);

            fallbackPolicy.Execute(() => { return ResultPrimitive.Fault; }, new { key = "value1" }.AsDictionary())
                .Should().Be(ResultPrimitive.Substitute);

            fallbackPolicy.Execute(() => { return ResultPrimitive.FaultAgain; }, new { key = "value2" }.AsDictionary())
                .Should().Be(ResultPrimitive.Substitute);

            contextData.Count.Should().Be(2);
            contextData.Keys.Should().Contain(ResultPrimitive.Fault);
            contextData.Keys.Should().Contain(ResultPrimitive.FaultAgain);
            contextData[ResultPrimitive.Fault].Should().Be("value1");
            contextData[ResultPrimitive.FaultAgain].Should().Be("value2");
        }

        [Fact]
        public void Context_should_be_empty_if_execute_not_called_with_any_context_data()
        {
            Context capturedContext = null;
            bool onFallbackExecuted = false;

            Func<ResultPrimitive> fallbackAction = () => ResultPrimitive.Substitute;
            Action<DelegateResult<ResultPrimitive>, Context> onFallback = (ex, ctx) => { onFallbackExecuted = true; capturedContext = ctx; };

            FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .Fallback(fallbackAction, onFallback);

            fallbackPolicy.RaiseResultSequence(ResultPrimitive.Fault);

            onFallbackExecuted.Should().BeTrue();
            capturedContext.Should().BeEmpty();
        }

        #endregion

    }
}
