using System;
using System.Threading.Tasks;
using Polly.Retry;
using Polly.Strategy;
using Xunit;

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

    public static readonly TheoryData<TimeSpan> ValidDelays = new() { TimeSpan.Zero, TimeSpan.FromMilliseconds(123) };

    [MemberData(nameof(ValidDelays))]
    [Theory]
    public async Task GeneratorRegistered_EnsureValueNotIgnored(TimeSpan delay)
    {
        var result = await new RetryDelayGenerator()
            .SetGenerator<int>((_, _) => delay)
            .CreateHandler()!
            .Generate(new Outcome<int>(0), new RetryDelayArguments(ResilienceContext.Get(), 0));

        result.Should().Be(delay);
    }
}
