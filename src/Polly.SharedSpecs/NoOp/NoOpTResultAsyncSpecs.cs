using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.NoOp;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.NoOp
{
    public class NoOpTResultAsyncSpecs
    {
        [Fact]
        public void Should_execute_user_delegate()
        {
            NoOpPolicy<int?> policy = Policy.NoOpAsync<int?>();
            int? result = null;

            policy.Awaiting(async p => result = await p.ExecuteAsync(() => Task.FromResult((int?)10)))
                .ShouldNotThrow();

            result.HasValue.Should().BeTrue();
            result.Should().Be(10);
        }

        [Fact]
        public void Should_execute_user_delegate_without_adding_extra_cancellation_behaviour()
        {
            NoOpPolicy<int?> policy = Policy.NoOpAsync<int?>();
            int? result = null;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Awaiting(async p => result = await p.ExecuteAsync(ct => Task.FromResult((int?)10), cts.Token))
                    .ShouldNotThrow();
            }

            result.HasValue.Should().BeTrue();
            result.Should().Be(10);
        }
    }
}
