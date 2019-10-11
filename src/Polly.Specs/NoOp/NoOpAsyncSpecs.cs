using System.Threading;
using FluentAssertions;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.NoOp
{
    public class NoOpAsyncSpecs
    {
        [Fact]
        public void Should_execute_user_delegate()
        {
            var policy = Policy.NoOpAsync();
            bool executed = false;

            policy.Awaiting(async p => await p.ExecuteAsync(() => { executed = true; return TaskHelper.EmptyTask; }))
                .Should().NotThrow();

            executed.Should().BeTrue();
        }

        [Fact]
        public void Should_execute_user_delegate_without_adding_extra_cancellation_behaviour()
        {
            var policy = Policy.NoOpAsync();

            bool executed = false;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Awaiting(async p => await p.ExecuteAsync(
                    ct => { executed = true; return TaskHelper.EmptyTask; }, cts.Token))
                    .Should().NotThrow();
            }

            executed.Should().BeTrue();
        }
    }
}
