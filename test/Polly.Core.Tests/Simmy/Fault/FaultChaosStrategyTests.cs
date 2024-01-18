using Polly.Simmy;
using Polly.Simmy.Fault;
using Polly.Telemetry;

namespace Polly.Core.Tests.Simmy.Fault;

public class FaultChaosStrategyTests
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly List<TelemetryEventArguments<object, object>> _args = [];

    public FaultChaosStrategyTests() => _telemetry = TestUtilities.CreateResilienceTelemetry(arg => _args.Add(arg));

    public static List<object[]> FaultCtorTestCases =>
        new()
        {
                new object[] { null!, "Value cannot be null. (Parameter 'options')", typeof(ArgumentNullException) },
                new object[]
                {
                    new FaultStrategyOptions
                    {
                        InjectionRate = 1,
                        Enabled = true,
                    },
                    "Either Fault or FaultGenerator is required.",
                    typeof(InvalidOperationException)
                },
        };

    [Theory]
    [MemberData(nameof(FaultCtorTestCases))]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
    public void FaultInvalidCtor(object options, string expectedMessage, Type expectedException)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    {
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            var _ = new FaultChaosStrategy((FaultStrategyOptions)options, _telemetry);
        }
        catch (Exception ex)
        {
            Assert.IsType(expectedException, ex);
#if !NET481
            Assert.Equal(expectedMessage, ex.Message);
#endif
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    [Fact]
    public void Given_not_enabled_should_not_inject_fault()
    {
        var userDelegateExecuted = false;
        var fault = new InvalidOperationException("Dummy exception");

        var options = new FaultStrategyOptions
        {
            InjectionRate = 0.6,
            Enabled = false,
            Randomizer = () => 0.5,
            FaultGenerator = _ => new ValueTask<Exception?>(fault)
        };

        var sut = CreateSut(options);
        sut.Execute(() => { userDelegateExecuted = true; });

        userDelegateExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Given_not_enabled_should_not_inject_fault_and_return_outcome()
    {
        var userDelegateExecuted = false;
        var fault = new InvalidOperationException("Dummy exception");

        var options = new FaultStrategyOptions
        {
            InjectionRate = 0.6,
            Enabled = false,
            Randomizer = () => 0.5,
            FaultGenerator = _ => new ValueTask<Exception?>(fault)
        };

        var sut = new ResiliencePipelineBuilder<HttpStatusCode>().AddChaosFault(options).Build();
        var response = await sut.ExecuteAsync(async _ =>
        {
            userDelegateExecuted = true;
            return await Task.FromResult(HttpStatusCode.OK);
        });

        response.Should().Be(HttpStatusCode.OK);
        userDelegateExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_should_inject_fault_instead_returning_outcome()
    {
        var onFaultInjected = false;
        var userDelegateExecuted = false;
        var exceptionMessage = "Dummy exception";
        var fault = new InvalidOperationException(exceptionMessage);

        var options = new FaultStrategyOptions
        {
            InjectionRate = 0.6,
            Enabled = true,
            Randomizer = () => 0.5,
            FaultGenerator = _ => new ValueTask<Exception?>(fault),
            OnFaultInjected = args =>
            {
                args.Context.Should().NotBeNull();
                args.Context.CancellationToken.IsCancellationRequested.Should().BeFalse();
                onFaultInjected = true;
                return default;
            }
        };

        var sut = new ResiliencePipelineBuilder<HttpStatusCode>().AddChaosFault(options).Build();
        await sut.Invoking(s => s.ExecuteAsync(async _ =>
        {
            userDelegateExecuted = true;
            return await Task.FromResult(HttpStatusCode.OK);
        }).AsTask())
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(exceptionMessage);

        userDelegateExecuted.Should().BeFalse();
        onFaultInjected.Should().BeTrue();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_should_inject_fault()
    {
        var onFaultInjected = false;
        var userDelegateExecuted = false;
        var exceptionMessage = "Dummy exception";
        var fault = new InvalidOperationException(exceptionMessage);

        var options = new FaultStrategyOptions
        {
            InjectionRate = 0.6,
            Enabled = true,
            Randomizer = () => 0.5,
            FaultGenerator = _ => new ValueTask<Exception?>(fault),
            OnFaultInjected = args =>
            {
                args.Context.Should().NotBeNull();
                args.Context.CancellationToken.IsCancellationRequested.Should().BeFalse();
                onFaultInjected = true;
                return default;
            }
        };

        var sut = CreateSut(options);
        await sut.Invoking(s => s.ExecuteAsync(async _ =>
        {
            userDelegateExecuted = true;
            return await Task.FromResult(200);
        }).AsTask())
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(exceptionMessage);

        userDelegateExecuted.Should().BeFalse();
        _args.Should().HaveCount(1);
        _args[0].Arguments.Should().BeOfType<OnFaultInjectedArguments>();
        _args[0].Event.EventName.Should().Be(FaultConstants.OnFaultInjectedEvent);
        onFaultInjected.Should().BeTrue();
    }

    [Fact]
    public void Given_enabled_and_randomly_not_within_threshold_should_not_inject_fault()
    {
        var userDelegateExecuted = false;
        var fault = new InvalidOperationException("Dummy exception");

        var options = new FaultStrategyOptions
        {
            InjectionRate = 0.3,
            Enabled = true,
            Randomizer = () => 0.5,
            FaultGenerator = _ => new ValueTask<Exception?>(fault)
        };

        var sut = CreateSut(options);
        var result = sut.Execute(_ =>
        {
            userDelegateExecuted = true;
            return 200;
        });

        result.Should().Be(200);
        userDelegateExecuted.Should().BeTrue();
    }

    [Fact]
    public void Given_enabled_and_randomly_within_threshold_should_not_inject_fault_when_exception_is_null()
    {
        var userDelegateExecuted = false;
        var options = new FaultStrategyOptions
        {
            InjectionRate = 0.6,
            Enabled = true,
            Randomizer = () => 0.5,
            FaultGenerator = (_) => new ValueTask<Exception?>(Task.FromResult<Exception?>(null))
        };

        var sut = CreateSut(options);
        sut.Execute(_ =>
        {
            userDelegateExecuted = true;
        });

        userDelegateExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Should_not_execute_user_delegate_when_it_was_cancelled_running_the_strategy()
    {
        var userDelegateExecuted = false;
        var fault = new InvalidOperationException("Dummy exception");

        using var cts = new CancellationTokenSource();
        var options = new FaultStrategyOptions
        {
            InjectionRate = 0.3,
            EnabledGenerator = (_) =>
            {
                cts.Cancel();
                return new ValueTask<bool>(true);
            },
            Randomizer = () => 0.5,
            FaultGenerator = _ => new ValueTask<Exception?>(fault)
        };

        var sut = CreateSut(options);
        await sut.Invoking(s => s.ExecuteAsync(async _ =>
        {
            userDelegateExecuted = true;
            return await Task.FromResult(1);
        }, cts.Token)
        .AsTask())
            .Should()
            .ThrowAsync<OperationCanceledException>();

        userDelegateExecuted.Should().BeFalse();
    }

    private ResiliencePipeline CreateSut(FaultStrategyOptions options) =>
        new FaultChaosStrategy(options, _telemetry).AsPipeline();
}

/// <summary>
/// Borrowing this from the actual dotnet standard implementation since it is not available in the net481.
/// </summary>
public enum HttpStatusCode
{
    // Summary:
    //     Equivalent to HTTP status 200. System.Net.HttpStatusCode.OK indicates that the
    //     request succeeded and that the requested information is in the response. This
    //     is the most common status code to receive.
    OK = 200,

    // Summary:
    //     Equivalent to HTTP status 429. System.Net.HttpStatusCode.TooManyRequests indicates
    //     that the user has sent too many requests in a given amount of time.
    TooManyRequests = 429,
}
