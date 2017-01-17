using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.NoOp
{
    public class NoOpTResultSpecs
    {
        [Fact]
        public void Should_not_throw_when_executing()
        {
            var policy = Policy.NoOp<int>();

            policy.Invoking(x => x.Execute(() => 10))
                .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_execute_if_cancellationtoken_cancelled_before_delegate_reached()
        {
            var policy = Policy.NoOp<int>();

            bool executed = false;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Invoking(p => p.Execute(ct => { executed = true; return 10; }, cts.Token))
                   .ShouldThrow<OperationCanceledException>();
            }

            executed.Should().BeFalse();
        }
    }
}
