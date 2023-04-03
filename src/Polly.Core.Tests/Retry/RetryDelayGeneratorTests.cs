using System;
using System.Threading.Tasks;
using Polly.Retry;
using Polly.Strategy;

namespace Polly.Core.Tests.Retry;

public class RetryDelayGeneratorTests
{
    [Fact]
    public async Task NoGeneratorRegisteredForType_EnsureDefaultValue()
    {
        var result = await new RetryDelayGenerator()
            .SetGenerator<int>((_, _) => TimeSpan.Zero)
            .CreateHandler()!
            .Generate(new Outcome<bool>(true), new RetryDelayArguments(ResilienceContext.Get(), 0));

        result.Should().Be(TimeSpan.MinValue);
    }

    [Fact]
    public async Task GeneratorRegistered_EnsureValueNotIgnored()
    {
        var result = await new RetryDelayGenerator()
            .SetGenerator<int>((_, _) => TimeSpan.FromMilliseconds(123))
            .CreateHandler()!
            .Generate(new Outcome<int>(0), new RetryDelayArguments(ResilienceContext.Get(), 0));

        result.Should().Be(TimeSpan.FromMilliseconds(123));
    }
}
