using Polly.Simmy;
using Polly.Simmy.Outcomes;
using Polly.Telemetry;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OutcomeChaosStrategyTests
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly List<TelemetryEventArguments<object, object>> _args = new();

    public OutcomeChaosStrategyTests() => _telemetry = TestUtilities.CreateResilienceTelemetry(arg => _args.Add(arg));

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

    public static List<object[]> ResultCtorTestCases =>
        new()
        {
                new object[] { null!, "Value cannot be null. (Parameter 'options')", typeof(ArgumentNullException) },
                new object[]
                {
                    new OutcomeStrategyOptions<int>
                    {
                        InjectionRate = 1,
                        Enabled = true,
                    },
                    "Either Outcome or OutcomeGenerator is required.",
                    typeof(InvalidOperationException)
                },
        };

    [Theory]
    [MemberData(nameof(FaultCtorTestCases))]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
    public void FaultInvalidCtor(FaultStrategyOptions options, string expectedMessage, Type expectedException)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    {
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            var _ = new OutcomeChaosStrategy<object>(options, _telemetry);
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

    [Theory]
    [MemberData(nameof(ResultCtorTestCases))]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
    public void ResultInvalidCtor(OutcomeStrategyOptions<int> options, string expectedMessage, Type expectedException)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    {
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            var _ = new OutcomeChaosStrategy<int>(options, _telemetry);
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
            Fault = fault
        };

        var sut = new ResiliencePipelineBuilder().AddChaosFault(options).Build();
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
            Fault = fault
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
            Fault = fault,
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
            Fault = fault,
            OnFaultInjected = args =>
            {
                args.Context.Should().NotBeNull();
                args.Context.CancellationToken.IsCancellationRequested.Should().BeFalse();
                onFaultInjected = true;
                return default;
            }
        };

        var sut = CreateSut<int>(options);
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
        _args[0].Event.EventName.Should().Be(OutcomeConstants.OnFaultInjectedEvent);
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
            Fault = fault
        };

        var sut = CreateSut<int>(options);
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

        var sut = new ResiliencePipelineBuilder().AddChaosFault(options).Build();
        sut.Execute(_ =>
        {
            userDelegateExecuted = true;
        });

        userDelegateExecuted.Should().BeTrue();
    }

    [Fact]
    public void Given_not_enabled_should_not_inject_result()
    {
        var userDelegateExecuted = false;
        var fakeResult = HttpStatusCode.TooManyRequests;

        var options = new OutcomeStrategyOptions<HttpStatusCode>
        {
            InjectionRate = 0.6,
            Enabled = false,
            Randomizer = () => 0.5,
            Outcome = new Outcome<HttpStatusCode>(fakeResult)
        };

        var sut = CreateSut(options);
        var response = sut.Execute(() => { userDelegateExecuted = true; return HttpStatusCode.OK; });

        response.Should().Be(HttpStatusCode.OK);
        userDelegateExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_should_inject_result()
    {
        var onResultInjected = false;
        var userDelegateExecuted = false;
        var fakeResult = HttpStatusCode.TooManyRequests;

        var options = new OutcomeStrategyOptions<HttpStatusCode>
        {
            InjectionRate = 0.6,
            Enabled = true,
            Randomizer = () => 0.5,
            Outcome = new Outcome<HttpStatusCode>(fakeResult),
            OnOutcomeInjected = args =>
            {
                args.Context.Should().NotBeNull();
                args.Context.CancellationToken.IsCancellationRequested.Should().BeFalse();
                onResultInjected = true;
                return default;
            }
        };

        var sut = CreateSut(options);
        var response = await sut.ExecuteAsync(async _ =>
        {
            userDelegateExecuted = true;
            return await Task.FromResult(HttpStatusCode.OK);
        });

        response.Should().Be(fakeResult);
        userDelegateExecuted.Should().BeFalse();

        _args.Should().HaveCount(1);
        _args[0].Arguments.Should().BeOfType<OnOutcomeInjectedArguments<HttpStatusCode>>();
        _args[0].Event.EventName.Should().Be(OutcomeConstants.OnOutcomeInjectedEvent);
        onResultInjected.Should().BeTrue();
    }

    [Fact]
    public void Given_enabled_and_randomly_not_within_threshold_should_not_inject_result()
    {
        var userDelegateExecuted = false;
        var fakeResult = HttpStatusCode.TooManyRequests;

        var options = new OutcomeStrategyOptions<HttpStatusCode>
        {
            InjectionRate = 0.3,
            Enabled = false,
            Randomizer = () => 0.5,
            Outcome = new Outcome<HttpStatusCode>(fakeResult)
        };

        var sut = CreateSut(options);
        var response = sut.Execute(_ =>
        {
            userDelegateExecuted = true;
            return HttpStatusCode.OK;
        });

        response.Should().Be(HttpStatusCode.OK);
        userDelegateExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_should_inject_result_even_as_null()
    {
        var userDelegateExecuted = false;
        var options = new OutcomeStrategyOptions<HttpStatusCode?>
        {
            InjectionRate = 0.6,
            Enabled = true,
            Randomizer = () => 0.5,
            Outcome = Outcome.FromResult<HttpStatusCode?>(null)
        };

        var sut = CreateSut(options);
        var response = await sut.ExecuteAsync<HttpStatusCode?>(async _ =>
        {
            userDelegateExecuted = true;
            return await Task.FromResult(HttpStatusCode.OK);
        });

        response.Should().Be(null);
        userDelegateExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_not_execute_user_delegate_when_it_was_cancelled_running_the_strategy()
    {
        var userDelegateExecuted = false;

        using var cts = new CancellationTokenSource();
        var options = new OutcomeStrategyOptions<HttpStatusCode>
        {
            InjectionRate = 0.6,
            Randomizer = () => 0.5,
            EnabledGenerator = (_) =>
            {
                cts.Cancel();
                return new ValueTask<bool>(true);
            },
            Outcome = Outcome.FromResult(HttpStatusCode.TooManyRequests)
        };

        var sut = CreateSut(options);
        await sut.Invoking(s => s.ExecuteAsync(async _ =>
        {
            userDelegateExecuted = true;
            return await Task.FromResult(HttpStatusCode.OK);
        }, cts.Token)
        .AsTask())
            .Should()
            .ThrowAsync<OperationCanceledException>();

        userDelegateExecuted.Should().BeFalse();
    }

    private ResiliencePipeline<TResult> CreateSut<TResult>(OutcomeStrategyOptions<TResult> options) =>
        new OutcomeChaosStrategy<TResult>(options, _telemetry).AsPipeline();

    private ResiliencePipeline<TResult> CreateSut<TResult>(FaultStrategyOptions options) =>
        new OutcomeChaosStrategy<TResult>(options, _telemetry).AsPipeline();
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
