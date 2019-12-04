using System;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Specs.Helpers.Custom.AddBehaviourIfHandle;
using Polly.Specs.Helpers.Custom.PreExecute;
using Xunit;

namespace Polly.Specs.Custom
{
    public class CustomAsyncSpecs
    {
        [Fact]
        public void Should_be_able_to_construct_active_policy()
        {
            Action construct = () =>
            {
                AsyncPreExecutePolicy policy = AsyncPreExecutePolicy.CreateAsync(async () =>
                {
                    // Placeholder for more substantive async work.
                    Console.WriteLine("Do something");
                    await Task.CompletedTask;
                });
            };

            construct.Should().NotThrow();
        }

        [Fact]
        public void Active_policy_should_execute()
        {
            bool preExecuted = false;
            AsyncPreExecutePolicy policy = AsyncPreExecutePolicy.CreateAsync(() => { preExecuted = true; return Task.CompletedTask; });

            bool executed = false;

            policy.Awaiting(x => x.ExecuteAsync(() => { executed = true; return Task.CompletedTask; }))
                .Should().NotThrow();

            executed.Should().BeTrue();
            preExecuted.Should().BeTrue();
        }

        [Fact]
        public void Should_be_able_to_construct_reactive_policy()
        {
            Action construct = () =>
            {
                AsyncAddBehaviourIfHandlePolicy policy = Policy.Handle<Exception>().WithBehaviourAsync(async ex =>
                {
                    // Placeholder for more substantive async work.
                    Console.WriteLine("Handling " + ex.Message);
                    await Task.CompletedTask;
                });
            };

            construct.Should().NotThrow();
        }

        [Fact]
        public void Reactive_policy_should_handle_exception()
        {
            Exception handled = null;
            AsyncAddBehaviourIfHandlePolicy policy = Policy.Handle<InvalidOperationException>().WithBehaviourAsync(async ex => { handled = ex; await Task.CompletedTask; });

            Exception toThrow = new InvalidOperationException();
            bool executed = false;

            policy.Awaiting(x => x.ExecuteAsync(() =>
            {
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
            AsyncAddBehaviourIfHandlePolicy policy = Policy.Handle<InvalidOperationException>().WithBehaviourAsync(async ex => { handled = ex; await Task.CompletedTask; });

            Exception toThrow = new NotImplementedException();
            bool executed = false;

            policy.Awaiting(x => x.ExecuteAsync(() =>
                {
                    executed = true;
                    throw toThrow;
                }))
                .Should().Throw<Exception>().Which.Should().Be(toThrow);

            executed.Should().BeTrue();
            handled.Should().Be(null);
        }
    }
}
