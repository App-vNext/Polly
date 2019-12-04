﻿using System;
using FluentAssertions;
using Polly.Caching;
using Xunit;

namespace Polly.Specs.Caching
{
    public class ResultTtlSpecs
    {
        [Fact]
        public void Should_throw_when_func_is_null()
        {
            Action configure = () => new ResultTtl<object>((Func<object, Ttl>)null);

            configure.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("ttlFunc");
        }

        [Fact]
        public void Should_throw_when_func_is_null_using_context()
        {
            Action configure = () => new ResultTtl<object>((Func<Context, object, Ttl>)null);

            configure.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("ttlFunc");
        }

        [Fact]
        public void Should_not_throw_when_func_is_set()
        {
            Action configure = () => new ResultTtl<object>(result => new Ttl());

            configure.Should().NotThrow();
        }

        [Fact]
        public void Should_not_throw_when_func_is_set_using_context()
        {
            Action configure = () => new ResultTtl<object>((context, result) => new Ttl());

            configure.Should().NotThrow();
        }

        [Fact]
        public void Should_return_func_result()
        {
            TimeSpan ttl = TimeSpan.FromMinutes(1);
            Func<dynamic, Ttl> func = result => new Ttl(result.Ttl);

            ResultTtl<dynamic> ttlStrategy = new ResultTtl<dynamic>(func);

            Ttl retrieved = ttlStrategy.GetTtl(new Context("someOperationKey"), new { Ttl = ttl });
            retrieved.Timespan.Should().Be(ttl);
            retrieved.SlidingExpiration.Should().BeFalse();
        }

        [Fact]
        public void Should_return_func_result_using_context()
        {
            const string specialKey = "specialKey";

            TimeSpan ttl = TimeSpan.FromMinutes(1);
            Func<Context, dynamic, Ttl> func = (context, result) => context.OperationKey == specialKey ? new Ttl(TimeSpan.Zero) : new Ttl(result.Ttl);

            ResultTtl<dynamic> ttlStrategy = new ResultTtl<dynamic>(func);

            ttlStrategy.GetTtl(new Context("someOperationKey"), new { Ttl = ttl }).Timespan.Should().Be(ttl);
            ttlStrategy.GetTtl(new Context(specialKey), new { Ttl = ttl }).Timespan.Should().Be(TimeSpan.Zero);
        }
    }
}
