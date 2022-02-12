using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.NoOp;
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

            Func<AsyncNoOpPolicy<int?>, Task> action = async p => result = await p.ExecuteAsync(() => Task.FromResult((int?)10));
            policy.Awaiting(action)
                .Should().NotThrowAsync();

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

                Func<AsyncNoOpPolicy<int?>, Task> action = async p => result = await p.ExecuteAsync(_ => Task.FromResult((int?)10), cts.Token);
                policy.Awaiting(action)
                    .Should().NotThrowAsync();
            }

            result.HasValue.Should().BeTrue();
            result.Should().Be(10);
        }
    }
}
