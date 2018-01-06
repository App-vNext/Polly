using System;
using FluentAssertions;
using Polly.Caching;
using Polly.Specs.Helpers.Caching;
using Xunit;

namespace Polly.Specs.Caching
{
    public class ResultTtlSpecs
    {
        [Fact]
        public void Should_throw_when_func_is_null()
        {
            Action configure = () => new ResultTtl<object>(null);

            configure.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("ttlFunc");
        }

        [Fact]
        public void Should_not_throw_when_func_is_set()
        {
            Action configure = () => new ResultTtl<object>((result) => new Ttl());

            configure.ShouldNotThrow();
        }

        [Fact]
        public void Should_return_func_result()
        {
            TimeSpan ttl = TimeSpan.FromMinutes(1);
            Func<dynamic, Ttl> func = (result) => { return new Ttl(result.Ttl); };

            ResultTtl<dynamic> ttlStrategy = new ResultTtl<dynamic>(func);

            Ttl retrieved = ttlStrategy.GetTtl(new Context("someExecutionKey"), new { Ttl = ttl });
            retrieved.Timespan.Should().Be(ttl);
            retrieved.SlidingExpiration.Should().BeFalse();
        }
        
    }
}
