﻿using System.Threading;
using FluentAssertions;
using Polly.NoOp;
using Xunit;

namespace Polly.Specs.NoOp
{
    public class NoOpSpecs
    {
        [Fact]
        public void Should_execute_user_delegate()
        {
            NoOpPolicy policy = Policy.NoOp();
            bool executed = false;

            policy.Invoking(x => x.Execute(() => { executed = true; }))
                .Should().NotThrow();

            executed.Should().BeTrue();
        }

        [Fact]
        public void Should_execute_user_delegate_without_adding_extra_cancellation_behaviour()
        {
            NoOpPolicy policy = Policy.NoOp();
            bool executed = false;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Invoking(p => p.Execute(ct => { executed = true; }, cts.Token))
                    .Should().NotThrow();
            }

            executed.Should().BeTrue();
        }
    }
}
