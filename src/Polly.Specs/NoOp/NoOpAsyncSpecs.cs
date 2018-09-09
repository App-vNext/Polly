using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Polly.Specs.NoOp
{
    public class NoOpAsyncSpecs
    {
        [Fact]
        public void Should_execute_user_delegate()
        {
            var policy = Policy.NoOpAsync();
            var executed = false;

            policy.Awaiting(async p => await p.ExecuteAsync(() => { executed = true; return Task.CompletedTask; }))
                .ShouldNotThrow();

            executed.Should().BeTrue();
        }

        [Fact]
        public void Should_execute_user_delegate_without_adding_extra_cancellation_behaviour()
        {
            var policy = Policy.NoOpAsync();

            var executed = false;

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Awaiting(async p => await p.ExecuteAsync(
                    ct => { executed = true; return Task.CompletedTask; }, cts.Token))
                    .ShouldNotThrow();
            }

            executed.Should().BeTrue();
        }
    }
}
