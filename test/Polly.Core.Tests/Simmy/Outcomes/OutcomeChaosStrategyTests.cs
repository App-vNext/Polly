using Moq;
using Polly.Simmy.Outcomes;
using Polly.Telemetry;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OutcomeChaosStrategyTests
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly Mock<DiagnosticSource> _diagnosticSource = new();

    public OutcomeChaosStrategyTests() => _telemetry = TestUtilities.CreateResilienceTelemetry(_diagnosticSource.Object);

    public static List<object[]> FaultCtorTestCases =>
    new()
    {
            new object[] { null!, "Value cannot be null. (Parameter 'options')" },
            new object[] { new OutcomeStrategyOptions<Exception>
            {
                InjectionRate = 1,
                Enabled = true,
            }, "Either Outcome or OutcomeGenerator is required. (Parameter 'Outcome')" },
    };

    [Theory]
    [MemberData(nameof(FaultCtorTestCases))]
    public void FaultInvalidCtor(OutcomeStrategyOptions<Exception> options, string message)
    {
        Action act = () =>
        {
            var _ = new OutcomeChaosStrategy(options, _telemetry);
        };

#if NET481
act.Should()
            .Throw<ArgumentNullException>();
#else
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage(message);
#endif
    }

    [Fact]
    public void Given_not_enabled_should_not_inject_fault()
    {
        var userDelegateExecuted = false;
        var fault = new InvalidOperationException("Dummy exception");

        var options = new OutcomeStrategyOptions<Exception>
        {
            InjectionRate = 0.6,
            Enabled = false,
            Randomizer = () => 0.5,
            Outcome = new Outcome<Exception>(fault)
        };

        var sut = CreateSut(options);
        sut.Execute(() => { userDelegateExecuted = true; });

        userDelegateExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_should_inject_fault()
    {
        var userDelegateExecuted = false;
        var exceptionMessage = "Dummy exception";
        var fault = new InvalidOperationException(exceptionMessage);

        var options = new OutcomeStrategyOptions<Exception>
        {
            InjectionRate = 0.6,
            Enabled = true,
            Randomizer = () => 0.5,
            Outcome = new Outcome<Exception>(fault)
        };

        var sut = CreateSut<int>(options);
        await sut.Invoking(s => s.ExecuteAsync(async _ =>
        {
            userDelegateExecuted = true;
            return await Task.FromResult(new Outcome<int>(200));
        }).AsTask())
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(exceptionMessage);

        userDelegateExecuted.Should().BeFalse();
    }

    [Fact]
    public void Given_enabled_and_randomly_not_within_threshold_should_not_inject_fault()
    {
        var userDelegateExecuted = false;
        var fault = new InvalidOperationException("Dummy exception");

        var options = new OutcomeStrategyOptions<Exception>
        {
            InjectionRate = 0.3,
            Enabled = true,
            Randomizer = () => 0.5,
            Outcome = new Outcome<Exception>(fault)
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
        var options = new OutcomeStrategyOptions<Exception>
        {
            InjectionRate = 0.6,
            Enabled = true,
            Randomizer = () => 0.5,
            OutcomeGenerator = (_) => new ValueTask<Outcome<Exception>?>(Task.FromResult<Outcome<Exception>?>(null))
        };

        var sut = CreateSut(options);
        sut.Execute(_ =>
        {
            userDelegateExecuted = true;
        });

        userDelegateExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Should_not_inject_fault_when_it_was_cancelled_running_the_fault_generator()
    {
        var userDelegateExecuted = false;
        var fault = new InvalidOperationException("Dummy exception");

        using var cts = new CancellationTokenSource();
        var options = new OutcomeStrategyOptions<Exception>
        {
            InjectionRate = 0.6,
            Enabled = true,
            Randomizer = () => 0.5,
            OutcomeGenerator = (_) =>
            {
                cts.Cancel();
                return new ValueTask<Outcome<Exception>?>(new Outcome<Exception>(fault));
            }
        };

        var sut = CreateSut<int>(options);
        var restult = await sut.ExecuteAsync(async _ =>
        {
            userDelegateExecuted = true;
            return await Task.FromResult(200);
        }, cts.Token);

        restult.Should().Be(200);
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
        var userDelegateExecuted = false;
        var fakeResult = HttpStatusCode.TooManyRequests;

        var options = new OutcomeStrategyOptions<HttpStatusCode>
        {
            InjectionRate = 0.6,
            Enabled = true,
            Randomizer = () => 0.5,
            Outcome = new Outcome<HttpStatusCode>(fakeResult)
        };

        var sut = CreateSut(options);
        var response = await sut.ExecuteAsync(async _ =>
        {
            userDelegateExecuted = true;
            return await Task.FromResult(HttpStatusCode.OK);
        });

        response.Should().Be(fakeResult);
        userDelegateExecuted.Should().BeFalse();
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

    private OutcomeChaosStrategy<TResult> CreateSut<TResult>(OutcomeStrategyOptions<TResult> options) =>
        new(options, _telemetry);

    private OutcomeChaosStrategy<TResult> CreateSut<TResult>(OutcomeStrategyOptions<Exception> options) =>
        new(options, _telemetry);

    private OutcomeChaosStrategy CreateSut(OutcomeStrategyOptions<Exception> options) =>
        new(options, _telemetry);
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
