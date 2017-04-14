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
        public void Should_execute_user_delegate()
        {
            NoOpPolicy policy = Policy.NoOpAsync();
            bool executed = false;

            policy.Awaiting(async p => await p.ExecuteAsync(() => { executed = true; return TaskHelper.EmptyTask; }))
                .ShouldNotThrow();

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
                    .ShouldNotThrow();
            }

            executed.Should().BeTrue();
        }
    }
}
