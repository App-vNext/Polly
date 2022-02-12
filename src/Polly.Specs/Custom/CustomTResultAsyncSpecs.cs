using System;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Specs.Helpers.Custom.AddBehaviourIfHandle;
using Polly.Specs.Helpers.Custom.PreExecute;
using Xunit;

namespace Polly.Specs.Custom
{
    public class CustomTResultAsyncSpecs
    {
        [Fact]
        public void Should_be_able_to_construct_active_policy()
        {
            Action construct = () =>
            {
                AsyncPreExecutePolicy<ResultPrimitive> policy = AsyncPreExecutePolicy<ResultPrimitive>.CreateAsync(async () =>
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
            AsyncPreExecutePolicy<ResultPrimitive> policy = AsyncPreExecutePolicy<ResultPrimitive>.CreateAsync(() => { preExecuted = true; return Task.CompletedTask; });

            bool executed = false;
            policy.Awaiting(x => x.ExecuteAsync(async () => { executed = true; await Task.CompletedTask; return ResultPrimitive.Undefined; }))
                .Should().NotThrowAsync();

            executed.Should().BeTrue();
            preExecuted.Should().BeTrue();
        }

        [Fact]
        public void Should_be_able_to_construct_reactive_policy()
        {
            Action construct = () =>
            {
                AsyncAddBehaviourIfHandlePolicy<ResultPrimitive> policy = Policy.HandleResult<ResultPrimitive>(ResultPrimitive.Fault).WithBehaviourAsync(async outcome =>
                {
                    // Placeholder for more substantive async work.
                    Console.WriteLine("Handling " + outcome.Result);
                    await Task.CompletedTask;
                });
            };

            construct.Should().NotThrow();
        }

        [Fact]
        public async Task Reactive_policy_should_handle_result()
        {
            ResultPrimitive handled = ResultPrimitive.Undefined;
            AsyncAddBehaviourIfHandlePolicy<ResultPrimitive> policy = Policy
                .HandleResult<ResultPrimitive>(ResultPrimitive.Fault)
                .WithBehaviourAsync(async outcome => { handled = outcome.Result; await Task.CompletedTask; });

            ResultPrimitive toReturn = ResultPrimitive.Fault;
            bool executed = false;

            (await policy.ExecuteAsync(async () =>
                {
                    executed = true;
                    await Task.CompletedTask;
                    return toReturn;
                }))
                .Should().Be(toReturn);

            executed.Should().BeTrue();
            handled.Should().Be(toReturn);
        }

        [Fact]
        public async Task Reactive_policy_should_be_able_to_ignore_unhandled_result()
        {
            ResultPrimitive? handled = null;
            AsyncAddBehaviourIfHandlePolicy<ResultPrimitive> policy = Policy
                .HandleResult<ResultPrimitive>(ResultPrimitive.Fault)
                .WithBehaviourAsync(async outcome => { handled = outcome.Result; await Task.CompletedTask; });

            ResultPrimitive toReturn = ResultPrimitive.FaultYetAgain;
            bool executed = false;

            (await policy.ExecuteAsync(async () =>
                {
                    executed = true;
                    await Task.CompletedTask;
                    return toReturn;
                }))
                .Should().Be(toReturn);

            executed.Should().BeTrue();
            handled.Should().Be(null);
        }

    }
}
