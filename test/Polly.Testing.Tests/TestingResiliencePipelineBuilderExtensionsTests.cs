﻿using NSubstitute;

namespace Polly.Testing.Tests;

public class TestingResiliencePipelineBuilderExtensionsTests
{
    [Fact]
    public void WithTimeProvider_Ok()
    {
        var timeProvider = Substitute.For<TimeProvider>();
        var builder = new ResiliencePipelineBuilder().WithTimeProvider(timeProvider);

        builder
            .GetType()
            .GetProperty("TimeProvider", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(builder).Should().Be(timeProvider);
    }
}
