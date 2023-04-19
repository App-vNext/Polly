using System.ComponentModel.DataAnnotations;
using Polly.CircuitBreaker;
using Polly.Strategy;
using Polly.Utils;
using Xunit;

namespace Polly.Core.Tests.CircuitBreaker;

public class AdvancedCircuitBreakerOptionsTests
{
    [Fact]
    public void Ctor_Defaults()
    {
        var options = new AdvancedCircuitBreakerStrategyOptions();

        options.BreakDuration.Should().Be(TimeSpan.FromSeconds(5));
        options.FailureThreshold.Should().Be(0.1);
        options.MinimumThroughput.Should().Be(100);
        options.SamplingDuration.Should().Be(TimeSpan.FromSeconds(30));
        options.OnOpened.IsEmpty.Should().BeTrue();
        options.OnClosed.IsEmpty.Should().BeTrue();
        options.OnHalfOpened.IsEmpty.Should().BeTrue();
        options.ShouldHandle.IsEmpty.Should().BeTrue();
        options.StrategyType.Should().Be("CircuitBreaker");
        options.StrategyName.Should().BeEmpty();

        // now set to min values
        options.FailureThreshold = 0.001;
        options.BreakDuration = TimeSpan.FromMilliseconds(500);
        options.MinimumThroughput = 2;
        options.SamplingDuration = TimeSpan.FromMilliseconds(500);

        ValidationHelper.ValidateObject(options, "Dummy.");
    }

    [Fact]
    public void Ctor_Generic_Defaults()
    {
        var options = new AdvancedCircuitBreakerStrategyOptions<int>();

        options.BreakDuration.Should().Be(TimeSpan.FromSeconds(5));
        options.FailureThreshold.Should().Be(0.1);
        options.MinimumThroughput.Should().Be(100);
        options.SamplingDuration.Should().Be(TimeSpan.FromSeconds(30));
        options.OnOpened.IsEmpty.Should().BeTrue();
        options.OnClosed.IsEmpty.Should().BeTrue();
        options.OnHalfOpened.IsEmpty.Should().BeTrue();
        options.ShouldHandle.IsEmpty.Should().BeTrue();
        options.StrategyType.Should().Be("CircuitBreaker");
        options.StrategyName.Should().BeEmpty();

        // now set to min values
        options.FailureThreshold = 0.001;
        options.BreakDuration = TimeSpan.FromMilliseconds(500);
        options.MinimumThroughput = 2;
        options.SamplingDuration = TimeSpan.FromMilliseconds(500);

        ValidationHelper.ValidateObject(options, "Dummy.");
    }

    [Fact]
    public async Task AsNonGenericOptions_Ok()
    {
        bool onBreakCalled = false;
        bool onResetCalled = false;
        bool onHalfOpenCalled = false;

        var options = new AdvancedCircuitBreakerStrategyOptions<int>
        {
            BreakDuration = TimeSpan.FromSeconds(123),
            FailureThreshold = 23,
            SamplingDuration = TimeSpan.FromSeconds(124),
            MinimumThroughput = 6,
            StrategyType = "dummy-type",
            StrategyName = "dummy-name",
            OnOpened = new OutcomeEvent<OnCircuitOpenedArguments, int>().Register(() => onBreakCalled = true),
            OnClosed = new OutcomeEvent<OnCircuitClosedArguments, int>().Register(() => onResetCalled = true),
            OnHalfOpened = new NoOutcomeEvent<OnCircuitHalfOpenedArguments>().Register(() => onHalfOpenCalled = true),
            ShouldHandle = new OutcomePredicate<CircuitBreakerPredicateArguments, int>().HandleException<InvalidOperationException>(),
            ManualControl = new CircuitBreakerManualControl(),
            StateProvider = new CircuitBreakerStateProvider()
        };

        var converted = options.AsNonGenericOptions();

        // assert converted options
        converted.StrategyType.Should().Be("dummy-type");
        converted.StrategyName.Should().Be("dummy-name");
        converted.FailureThreshold.Should().Be(23);
        converted.BreakDuration.Should().Be(TimeSpan.FromSeconds(123));
        converted.SamplingDuration.Should().Be(TimeSpan.FromSeconds(124));
        converted.MinimumThroughput.Should().Be(6);
        converted.ManualControl.Should().Be(options.ManualControl);
        converted.StateProvider.Should().Be(options.StateProvider);

        var context = ResilienceContext.Get();

        (await converted.ShouldHandle.CreateHandler()!.ShouldHandleAsync(new Outcome<int>(new InvalidOperationException()), new CircuitBreakerPredicateArguments(context))).Should().BeTrue();

        await converted.OnClosed.CreateHandler()!.HandleAsync(new Outcome<int>(new InvalidOperationException()), new OnCircuitClosedArguments(context));
        onResetCalled.Should().BeTrue();

        await converted.OnOpened.CreateHandler()!.HandleAsync(new Outcome<int>(new InvalidOperationException()), new OnCircuitOpenedArguments(context, TimeSpan.Zero));
        onBreakCalled.Should().BeTrue();

        await converted.OnHalfOpened.CreateHandler()!(new OnCircuitHalfOpenedArguments(context));
        onHalfOpenCalled.Should().BeTrue();
    }

    [Fact]
    public void InvalidOptions_Validate()
    {
        var options = new AdvancedCircuitBreakerStrategyOptions<int>
        {
            BreakDuration = TimeSpan.FromMilliseconds(299),
            FailureThreshold = 0,
            SamplingDuration = TimeSpan.Zero,
            MinimumThroughput = 0,
            OnOpened = null!,
            OnClosed = null!,
            OnHalfOpened = null!,
            ShouldHandle = null!,
        };

        options
            .Invoking(o => ValidationHelper.ValidateObject(o, "Dummy."))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Dummy.

            Validation Errors:
            The field MinimumThroughput must be between 2 and 2147483647.
            The field SamplingDuration must be >= to 00:00:00.5000000.
            The field BreakDuration must be >= to 00:00:00.5000000.
            The ShouldHandle field is required.
            The OnClosed field is required.
            The OnOpened field is required.
            The OnHalfOpened field is required.
            """);
    }
}
