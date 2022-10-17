using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.RateLimit;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.RateLimit;

public class AsyncSlidingWindowRateLimiterTests : SlidingWindowRateLimiterTestsBase, IDisposable
{
    protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan)
    {
        return Policy.RateLimitAsync(numberOfExecutions, perTimeSpan, spreadUniformly:false);
    }
    
    public void Dispose()
    {
        SystemClock.Reset();
    }
    
    [Fact]
    public void Syntax_should_throw_for_perTimeSpan_zero()
    {
        Action invalidSyntax = () => GetPolicyViaSyntax(1, TimeSpan.Zero);

        invalidSyntax.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("perTimeSpan");
    }

    [Fact]
    public void Syntax_should_throw_for_perTimeSpan_infinite()
    {
        Action invalidSyntax = () => GetPolicyViaSyntax(1, System.Threading.Timeout.InfiniteTimeSpan);

        invalidSyntax.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("perTimeSpan");
    }

    [Fact]
    public void Syntax_should_throw_for_numberOfExecutions_negative()
    {
        Action invalidSyntax = () => GetPolicyViaSyntax(-1, TimeSpan.FromSeconds(1));

        invalidSyntax.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("numberOfExecutions");
    }

    [Fact]
    public void Syntax_should_throw_for_numberOfExecutions_zero()
    {
        Action invalidSyntax = () => GetPolicyViaSyntax(0, TimeSpan.FromSeconds(1));

        invalidSyntax.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("numberOfExecutions");
    }

    [Fact]
    public void Syntax_should_throw_for_perTimeSpan_negative()
    {
        Action invalidSyntax = () => GetPolicyViaSyntax(1, TimeSpan.FromTicks(-1));

        invalidSyntax.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("perTimeSpan");
    }

    [Fact]
    public async Task ShouldAllowAllExecutionsWithinThePolicyTimeSpanAndLimit()
    {
        FixClock();
            
        // 60 executions in 1 minute
        var rateLimiter = (AsyncRateLimitPolicy) GetPolicyViaSyntax(60, new TimeSpan(0, 1, 0));
        var tasks = new List<Task<bool>>();
        for (var i = 0; i < 60; i++)
        {
            await Task.Delay(10);
            tasks.Add(Task.Run(async Task<bool> () =>
            {
                try
                {
                    await rateLimiter.ExecuteAsync(() => Task.CompletedTask);
                }
                catch (RateLimitRejectedException)
                {
                    return false;
                }

                return true;
            }));
        }
        await Task.WhenAll(tasks);
        tasks.FindAll(task => task.Result).Count.Should().Be(60);
        tasks.FindAll(task => !task.Result).Count.Should().Be(0);
    }

    [Fact]
    public async Task ShouldAllowNumberOfExecutionsPerPolicy_And_RejectTheRestWithinPolicyTimeSpan()
    {
        FixClock();
            
        // 60 executions in 1 minute
        var rateLimiter = (AsyncRateLimitPolicy) GetPolicyViaSyntax(60, new TimeSpan(0, 1, 0));
        var tasks = new List<Task<bool>>();
        for (var i = 0; i < 70; i++)
        {
            await Task.Delay(10);
            tasks.Add(Task.Run(async Task<bool> () =>
            {
                try
                {
                    await rateLimiter.ExecuteAsync(() => Task.CompletedTask);
                }
                catch (RateLimitRejectedException)
                {
                    return false;
                }

                return true;
            }));
        }
        await Task.WhenAll(tasks);
        tasks.FindAll(task => task.Result).Count.Should().Be(60);
        tasks.FindAll(task => !task.Result).Count.Should().Be(10);
    }

    [Fact]
    public async Task ShouldAllowAdditionalExecutionsOutsideThePolicyTimeSpanWithinLimit()
    {
        FixClock();
            
        // 60 executions in 1 minute
        var rateLimiter = (AsyncRateLimitPolicy) GetPolicyViaSyntax(60, new TimeSpan(0, 1, 0));
        var tasks = new List<Task<bool>>();
        for (var i = 0; i < 70; i++)
        {
            await Task.Delay(1000);
            tasks.Add(Task.Run(async Task<bool> () =>
            {
                try
                {
                    await rateLimiter.ExecuteAsync(() => Task.CompletedTask);
                }
                catch (RateLimitRejectedException)
                {
                    return false;
                }

                return true;
            }));
        }
        await Task.WhenAll(tasks);
        tasks.FindAll(task => task.Result).Count.Should().Be(70);
        tasks.FindAll(task => !task.Result).Count.Should().Be(0);
    }
}