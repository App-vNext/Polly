using System;
using FluentAssertions;
using Polly.Specs.Helpers.Custom.AddBehaviourIfHandle;
using Polly.Specs.Helpers.Custom.PreExecute;
using Xunit;

namespace Polly.Specs.Custom
{
    public class CustomSpecs
    {
        [Fact]
        public void Should_be_able_to_construct_active_policy()
        {
            Action construct = () =>
            {
                PreExecutePolicy policy = PreExecutePolicy.Create(() => Console.WriteLine("Do something"));
            };

            construct.Should().NotThrow();
        }

        [Fact]
        public void Active_policy_should_execute()
        {
            bool preExecuted = false;
            PreExecutePolicy policy = PreExecutePolicy.Create(() => preExecuted = true);

            bool executed = false;

            policy.Invoking(x => x.Execute(() => { executed = true; }))
                .Should().NotThrow();

            executed.Should().BeTrue();
            preExecuted.Should().BeTrue();
        }

        [Fact]
        public void Should_be_able_to_construct_reactive_policy()
        {
            Action construct = () =>
            {
                AddBehaviourIfHandlePolicy policy = Policy.Handle<Exception>().WithBehaviour(ex => Console.WriteLine("Handling " + ex.Message));
            };

            construct.Should().NotThrow();
        }

        [Fact]
        public void Reactive_policy_should_handle_exception()
        {
            Exception handled = null;
            AddBehaviourIfHandlePolicy policy = Policy.Handle<InvalidOperationException>().WithBehaviour(ex => handled = ex);

            Exception toThrow = new InvalidOperationException();
            bool executed = false;

            policy.Invoking(x => x.Execute(() => {
                    executed = true;
                    throw toThrow;
                }))
                .Should().Throw<Exception>().Which.Should().Be(toThrow);

            executed.Should().BeTrue();
            handled.Should().Be(toThrow);
        }

        [Fact]
        public void Reactive_policy_should_be_able_to_ignore_unhandled_exception()
        {
            Exception handled = null;
            AddBehaviourIfHandlePolicy policy = Policy.Handle<InvalidOperationException>().WithBehaviour(ex => handled = ex);

            Exception toThrow = new NotImplementedException();
            bool executed = false;

            policy.Invoking(x => x.Execute(() => {
                    executed = true;
                    throw toThrow;
                }))
                .Should().Throw<Exception>().Which.Should().Be(toThrow);

            executed.Should().BeTrue();
            handled.Should().Be(null);
        }
    }
}
