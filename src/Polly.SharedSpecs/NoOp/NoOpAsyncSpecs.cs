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
    public class NoOpAsyncSpecs
    {
        [Fact]
        public void Should_not_throw_when_executing()
        {
            NoOpPolicy policy = Policy.NoOpAsync();
            bool executed = false;

            policy.Awaiting(async p => await p.ExecuteAsync(() => { executed = true; return Task.FromResult(0); }))
                .ShouldNotThrow();

            executed.Should().BeTrue();
        }

        [Fact]
        public void Should_execute_if_cancellationtoken_cancelled_before_delegate_reached()
        {
            var policy = Policy.NoOpAsync();

            bool executed = false;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Awaiting(async p => await p.ExecuteAsync(
                    ct => { executed = true; return Task.FromResult(0); }, cts.Token))
                    .ShouldNotThrow();
            }

            executed.Should().BeTrue();
        }
    }
}
