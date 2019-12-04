using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Polly.Specs.NoOp
{
    public class NoOpTResultAsyncSpecs
    {
        [Fact]
        public void Should_execute_user_delegate()
        {
            var policy = Policy.NoOpAsync<int?>();
            int? result = null;

            policy.Awaiting(async p => result = await p.ExecuteAsync(() => Task.FromResult((int?)10)))
                .Should().NotThrow();

            result.HasValue.Should().BeTrue();
            result.Should().Be(10);
        }

        [Fact]
        public void Should_execute_user_delegate_without_adding_extra_cancellation_behaviour()
        {
            var policy = Policy.NoOpAsync<int?>();
            int? result = null;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Awaiting(async p => result = await p.ExecuteAsync(ct => Task.FromResult((int?)10), cts.Token))
                    .Should().NotThrow();
            }

            result.HasValue.Should().BeTrue();
            result.Should().Be(10);
        }
    }
}
