using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Polly.Monkey;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.SharedSpecs.Monkey
{
    public class MonkeySpec
    {

        [Fact]
        public void Should_introduce_fault()
        {
            Exception ex = new Exception();
            MonkeyPolicy policy = Policy.MonkeyPolicy(ex, 1, (ctx) => true);

            policy.Invoking(x => x)).ShouldThrow<Exception>();
        }
    }
}
