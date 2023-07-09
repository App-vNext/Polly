using Moq;
using Polly.Simmy.Outcomes;
using Polly.Telemetry;

namespace Polly.Core.Tests.Simmy.Outcomes;
public class OutcomeChaosStrategyTests
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly Mock<DiagnosticSource> _diagnosticSource = new();

    public OutcomeChaosStrategyTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(_diagnosticSource.Object);
    }

    [Fact]
    public async Task InjectFault_T_Result_enabled_should_throw_and_should_not_execute_user_delegate_async()
    {
        var outcome = new Outcome<Exception>(new InvalidOperationException("Dummy exception"));
        var userDelegateExecuted = false;

        // generic tests

        //var options = new OutcomeStrategyOptions<Exception>
        //{
        //    InjectionRate = 0.6,
        //    Enabled = true,
        //    Randomizer = () => 0.5,
        //    Outcome = outcome
        //};

        var sut = new ResilienceStrategyBuilder<VoidResult>()
            .AddFault(true, 1, new InvalidOperationException("Dummy exception"))
            .Build();

        //var sut = new OutcomeChaosStrategy<VoidResult>(options, _telemetry);

        await sut.Invoking(s => s.ExecuteAsync(_ =>
        {
            userDelegateExecuted = true;
            return default;
        }).AsTask())
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Dummy exception");

        userDelegateExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task InjectFault_enabled_should_throw_and_should_not_execute_user_delegate_async()
    {
        var userDelegateExecuted = false;

        // non-generic tests
        var nonGenericSut = new ResilienceStrategyBuilder()
            .AddFault(true, 1, new AggregateException("chimbo"))
            .Build();

        nonGenericSut.Invoking(s => s.Execute(_ => { userDelegateExecuted = true; }))
            .Should()
            .Throw<AggregateException>()
            .WithMessage("chimbo");

        userDelegateExecuted.Should().BeFalse();
    }
}
