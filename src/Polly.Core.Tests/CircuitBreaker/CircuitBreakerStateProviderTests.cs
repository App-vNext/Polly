using System;
using Polly.CircuitBreaker;
using Polly.Strategy;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerStateProviderTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var provider = new CircuitBreakerStateProvider();

        provider.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public void NotInitialized_EnsureDefaults()
    {
        var provider = new CircuitBreakerStateProvider();

        provider.CircuitState.Should().Be(CircuitState.Closed);
        provider.LastHandledOutcome.Should().Be(null);
    }

    [Fact]
    public async Task ResetAsync_NotInitialized_Throws()
    {
        using var control = new CircuitBreakerManualControl();

        await control
            .Invoking(c => c.CloseAsync(CancellationToken.None))
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void Initialize_Twice_Throws()
    {
        var provider = new CircuitBreakerStateProvider();
        provider.Initialize(() => CircuitState.Closed, () => null);

        provider
            .Invoking(c => c.Initialize(() => CircuitState.Closed, () => null))
            .Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void Initialize_Ok()
    {
        var provider = new CircuitBreakerStateProvider();
        var stateCalled = false;
        var exceptionCalled = false;

        provider.Initialize(
            () =>
            {
                stateCalled = true;
                return CircuitState.HalfOpen;
            },
            () =>
            {
                exceptionCalled = true;
                return new Outcome(typeof(string), new InvalidOperationException());
            });

        provider.CircuitState.Should().Be(CircuitState.HalfOpen);
        provider.LastHandledOutcome!.Value.Exception.Should().BeOfType<InvalidOperationException>();

        stateCalled.Should().BeTrue();
        exceptionCalled.Should().BeTrue();
    }
}
