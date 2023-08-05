using Microsoft.Extensions.Logging;
using Polly.Extensions.Telemetry;
using Polly.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

#pragma warning disable S103 // Lines should not be too long

[Collection("NonParallelizableTests")]
public class TelemetryResilienceStrategyTests : IDisposable
{
    private readonly FakeLogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDisposable _metering;
    private readonly List<MeteringEvent> _events = new();
    private Action<EnrichmentContext> _enricher = _ => { };

    public TelemetryResilienceStrategyTests()
    {
        _loggerFactory = TestUtilities.CreateLoggerFactory(out _logger);
        _metering = TestUtilities.EnablePollyMetering(_events);
    }

    [Fact]
    public void Ctor_Ok()
    {
        var duration = CreateStrategy().ExecutionDuration;

        duration.Unit.Should().Be("ms");
        duration.Description.Should().Be("The execution duration and execution results of resilience strategies.");
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Execute_EnsureLogged(bool healthy)
    {
        var healthString = healthy ? "Healthy" : "Unhealthy";
        var strategy = CreateStrategy();

        strategy.Execute(
            (c, _) =>
            {
                if (!healthy)
                {
                    ((List<ResilienceEvent>)c.ResilienceEvents).Add(new ResilienceEvent(ResilienceEventSeverity.Warning, "dummy"));
                }
            },
            ResilienceContextPool.Shared.Get("op-key"), string.Empty);

        var messages = _logger.GetRecords(new EventId(1, "StrategyExecuting")).ToList();
        messages.Should().HaveCount(1);
        messages[0].Message.Should().Be("Resilience strategy executing. Source: 'my-builder/my-instance', Operation Key: 'op-key', Result Type: 'void'");
        messages = _logger.GetRecords(new EventId(2, "StrategyExecuted")).ToList();
        messages.Should().HaveCount(1);
        messages[0].Message.Should().Match($"Resilience strategy executed. Source: 'my-builder/my-instance', Operation Key: 'op-key', Result Type: 'void', Result: 'void', Execution Health: '{healthString}', Execution Time: *ms");
        messages[0].LogLevel.Should().Be(healthy ? LogLevel.Debug : LogLevel.Warning);

        // verify reported state
        var coll = messages[0].State.Should().BeAssignableTo<IReadOnlyList<KeyValuePair<string, object>>>().Subject;
        coll.Count.Should().Be(8);
        coll.AsEnumerable().Should().HaveCount(8);
        (coll as IEnumerable).GetEnumerator().Should().NotBeNull();

        for (int i = 0; i < coll.Count; i++)
        {
            coll[i].Value.Should().NotBeNull();
        }

        coll.Invoking(c => c[coll.Count + 1]).Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void Execute_WithException_EnsureLogged()
    {
        var strategy = CreateStrategy();
        strategy.Invoking(s => s.Execute(_ => throw new InvalidOperationException("Dummy message."), ResilienceContextPool.Shared.Get("op-key"))).Should().Throw<InvalidOperationException>();

        var messages = _logger.GetRecords(new EventId(1, "StrategyExecuting")).ToList();
        messages.Should().HaveCount(1);
        messages[0].Message.Should().Be("Resilience strategy executing. Source: 'my-builder/my-instance', Operation Key: 'op-key', Result Type: 'void'");

        messages = _logger.GetRecords(new EventId(2, "StrategyExecuted")).ToList();
        messages.Should().HaveCount(1);
        messages[0].Message.Should().Match($"Resilience strategy executed. Source: 'my-builder/my-instance', Operation Key: 'op-key', Result Type: 'void', Result: 'Dummy message.', Execution Health: 'Healthy', Execution Time: *ms");
        messages[0].Exception.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void Execute_WithException_EnsureEnrichmentContextWithCorrectOutcome()
    {
        var strategy = CreateStrategy();

        _enricher = c =>
        {
            c.Outcome!.Value.Exception.Should().BeOfType<InvalidOperationException>();
        };
        strategy.Invoking(s => s.Execute(_ => throw new InvalidOperationException("Dummy message."))).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Execute_WithResult_EnsureEnrichmentContextWithCorrectOutcome()
    {
        var strategy = CreateStrategy();

        _enricher = c =>
        {
            c.Outcome!.Value.Result.Should().Be("dummy");
        };

        strategy.Execute(_ => "dummy");
    }

    [Fact]
    public void Execute_WithException_EnsureMetered()
    {
        var strategy = CreateStrategy();
        strategy.Invoking(s => s.Execute(_ => throw new InvalidOperationException("Dummy message."), ResilienceContextPool.Shared.Get("op-key"))).Should().Throw<InvalidOperationException>();

        var ev = _events.Single(v => v.Name == "strategy-execution-duration").Tags;

        ev.Count.Should().Be(6);
        ev["builder-instance"].Should().Be("my-instance");
        ev["operation-key"].Should().Be("op-key");
        ev["builder-name"].Should().Be("my-builder");
        ev["result-type"].Should().Be("void");
        ev["exception-name"].Should().Be("System.InvalidOperationException");
        ev["execution-health"].Should().Be("Healthy");
    }

    [Fact]
    public void Execute_Enrichers_Ok()
    {
        _enricher = context =>
        {
            context.Tags.Add(new KeyValuePair<string, object?>("my-custom-tag", "my-tag-value"));
        };
        var strategy = CreateStrategy();
        strategy.Execute(_ => true);

        var ev = _events.Single(v => v.Name == "strategy-execution-duration").Tags;

        ev.Count.Should().Be(5);
        ev["my-custom-tag"].Should().Be("my-tag-value");
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Execute_WithResult_EnsureMetered(bool healthy)
    {
        var strategy = CreateStrategy();
        strategy.Execute(
            (c, _) =>
            {
                if (!healthy)
                {
                    ((List<ResilienceEvent>)c.ResilienceEvents).Add(new ResilienceEvent(ResilienceEventSeverity.Warning, "dummy"));
                }

                return true;
            },
            ResilienceContextPool.Shared.Get("op-key"), string.Empty);

        var ev = _events.Single(v => v.Name == "strategy-execution-duration").Tags;

        ev.Count.Should().Be(5);
        ev["builder-instance"].Should().Be("my-instance");
        ev["operation-key"].Should().Be("op-key");
        ev["builder-name"].Should().Be("my-builder");
        ev["result-type"].Should().Be("Boolean");
        ev.Should().NotContainKey("exception-name");

        if (healthy)
        {
            ev["execution-health"].Should().Be("Healthy");
        }
        else
        {
            ev["execution-health"].Should().Be("Unhealthy");
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Execute_ExecutionHealth(bool healthy)
    {
        var strategy = CreateStrategy();
        strategy.Execute(
            (c, _) =>
            {
                if (healthy)
                {
                    ((List<ResilienceEvent>)c.ResilienceEvents).Add(new ResilienceEvent(ResilienceEventSeverity.Information, "dummy"));
                    ((List<ResilienceEvent>)c.ResilienceEvents).Add(new ResilienceEvent(ResilienceEventSeverity.Information, "dummy"));
                }
                else
                {
                    ((List<ResilienceEvent>)c.ResilienceEvents).Add(new ResilienceEvent(ResilienceEventSeverity.Information, "dummy"));
                    ((List<ResilienceEvent>)c.ResilienceEvents).Add(new ResilienceEvent(ResilienceEventSeverity.Warning, "dummy"));
                }

                return true;
            },
            ResilienceContextPool.Shared.Get(), string.Empty);

        var ev = _events.Single(v => v.Name == "strategy-execution-duration").Tags;

        if (healthy)
        {
            ev["execution-health"].Should().Be("Healthy");
        }
        else
        {
            ev["execution-health"].Should().Be("Unhealthy");
        }
    }

    private TelemetryResilienceStrategy CreateStrategy() => new("my-builder", "my-instance", _loggerFactory, (_, r) => r, new List<Action<EnrichmentContext>> { c => _enricher?.Invoke(c) });

    public void Dispose()
    {
        _metering.Dispose();
        _loggerFactory.Dispose();
    }
}
