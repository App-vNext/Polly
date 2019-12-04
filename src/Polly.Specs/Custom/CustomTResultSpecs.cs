using System;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Specs.Helpers.Custom.AddBehaviourIfHandle;
using Polly.Specs.Helpers.Custom.PreExecute;
using Xunit;

namespace Polly.Specs.Custom
{
    public class CustomTResultSpecs
    {
        [Fact]
        public void Should_be_able_to_construct_active_policy()
        {
            Action construct = () =>
            {
                PreExecutePolicy<ResultPrimitive> policy = PreExecutePolicy<ResultPrimitive>.Create(() => Console.WriteLine("Do something"));
            };

            construct.Should().NotThrow();
        }

        [Fact]
        public void Active_policy_should_execute()
        {
            bool preExecuted = false;
            PreExecutePolicy<ResultPrimitive> policy = PreExecutePolicy<ResultPrimitive>.Create(() => preExecuted = true);

            bool executed = false;

            policy.Invoking(x => x.Execute(() => {
                    executed = true;
                    return ResultPrimitive.Undefined;
                }))
                .Should().NotThrow();

            executed.Should().BeTrue();
            preExecuted.Should().BeTrue();
        }

        [Fact]
        public void Should_be_able_to_construct_reactive_policy()
        {
            Action construct = () =>
            {
                AddBehaviourIfHandlePolicy<ResultPrimitive> policy = Policy.HandleResult<ResultPrimitive>(ResultPrimitive.Fault).WithBehaviour(outcome => Console.WriteLine("Handling " + outcome.Result));
            };

            construct.Should().NotThrow();
        }

        [Fact]
        public void Reactive_policy_should_handle_result()
        {
            ResultPrimitive handled = ResultPrimitive.Undefined;
            AddBehaviourIfHandlePolicy<ResultPrimitive> policy = Policy.HandleResult<ResultPrimitive>(ResultPrimitive.Fault).WithBehaviour(outcome => handled = outcome.Result);

            ResultPrimitive toReturn = ResultPrimitive.Fault;
            bool executed = false;

            policy.Execute(() =>
                {
                    executed = true;
                    return toReturn;
                })
                .Should().Be(toReturn);

            executed.Should().BeTrue();
            handled.Should().Be(toReturn);
        }

        [Fact]
        public void Reactive_policy_should_be_able_to_ignore_unhandled_result()
        {
            ResultPrimitive? handled = null;
            AddBehaviourIfHandlePolicy<ResultPrimitive> policy = Policy.HandleResult<ResultPrimitive>(ResultPrimitive.Fault).WithBehaviour(outcome => handled = outcome.Result);

            ResultPrimitive toReturn = ResultPrimitive.FaultYetAgain;
            bool executed = false;

            policy.Execute(() =>
                {
                    executed = true;
                    return toReturn;
                })
                .Should().Be(toReturn);

            executed.Should().BeTrue();
            handled.Should().Be(null);
        }

    }
}
