using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Polly.Registry;
using Polly.Telemetry;

namespace Polly.Extensions.Tests;

public class ReloadableResiliencePipelineTests
{
    private static readonly ResiliencePropertyKey<string> TagKey = new("tests.tag");

    [InlineData(null)]
    [InlineData("custom-name")]
    [InlineData("")]
    [Theory]
    public void AddResiliencePipeline_EnsureReloadable(string? name)
    {
        var resList = new List<IDisposable>();
        var reloadableConfig = new ReloadableConfiguration();
        reloadableConfig.Reload(new() { { "tag", "initial-tag" } });
        var fakeListener = new FakeTelemetryListener();

        var configuration = new ConfigurationBuilder()
            .Add(reloadableConfig)
            .Build();

        var services = new ServiceCollection();

        if (name == null)
        {
            services.Configure<ReloadableStrategyOptions>(configuration)
                    .Configure<ReloadableStrategyOptions>(options => options.OptionsName = name);
        }
        else
        {
            services.Configure<ReloadableStrategyOptions>(name, configuration)
                    .Configure<ReloadableStrategyOptions>(name, options => options.OptionsName = name);
        }

        services.Configure<TelemetryOptions>(options => options.TelemetryListeners.Add(fakeListener));
        services.AddResiliencePipeline("my-pipeline", (builder, context) =>
        {
            builder.InstanceName = "my-instance";

            var options = context.GetOptions<ReloadableStrategyOptions>(name);
            options.ShouldNotBeNull();
            options.OptionsName.ShouldBe(name);

            context.EnableReloads<ReloadableStrategyOptions>(name);

            builder.AddStrategy(_ =>
            {
                var res = Substitute.For<IDisposable>();
                resList.Add(res);
                return new ReloadableStrategy(options.Tag, res);
            },
            options);
        });

        var serviceProvider = services.BuildServiceProvider();
        var pipeline = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>().GetPipeline("my-pipeline");
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);

        // initial
        pipeline.Execute(_ => "dummy", context);
        context.Properties.GetValue(TagKey, string.Empty).ShouldBe("initial-tag");

        // reloads
        for (int i = 0; i < 10; i++)
        {
            reloadableConfig.Reload(new() { { "tag", $"reload-{i}" } });
            pipeline.Execute(_ => "dummy", context);
            context.Properties.GetValue(TagKey, string.Empty).ShouldBe($"reload-{i}");
        }

        // check resource disposed
        resList.Count.ShouldBe(11);
        for (int i = 0; i < resList.Count - 1; i++)
        {
            resList[i].Received(1).Dispose();
        }

        resList[resList.Count - 1].Received(0).Dispose();

        // check disposal of service provider
        serviceProvider.Dispose();
        resList[resList.Count - 1].Received(1).Dispose();
        Should.Throw<ObjectDisposedException>(() => pipeline.Execute(() => { }));

        foreach (var ev in fakeListener.Events)
        {
            ev.Source.PipelineName.ShouldBe("my-pipeline");
            ev.Source.PipelineInstanceName.ShouldBe("my-instance");
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("custom-name")]
    public void EnableReloadsWithMonitor_EnsureReloadable(string? name)
    {
        var resList = new List<IDisposable>();
        var monitor = new FakeOptionsMonitor<ReloadableStrategyOptions>(
            new ReloadableStrategyOptions { Tag = "initial-tag", OptionsName = name });

        var services = new ServiceCollection();
        services.AddResiliencePipeline("my-pipeline", (builder, context) =>
        {
            context.EnableReloadsWithMonitor(monitor, name);

            var options = monitor.Get(name);
            builder.AddStrategy(_ =>
            {
                var res = Substitute.For<IDisposable>();
                resList.Add(res);
                return new ReloadableStrategy(options.Tag, res);
            },
            options);
        });

        var serviceProvider = services.BuildServiceProvider();
        var pipeline = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>().GetPipeline("my-pipeline");
        var ctx = ResilienceContextPool.Shared.Get(TestCancellation.Token);

        pipeline.Execute(_ => "dummy", ctx);
        ctx.Properties.GetValue(TagKey, string.Empty).ShouldBe("initial-tag");

        monitor.TriggerChange(new ReloadableStrategyOptions { Tag = "reloaded-tag", OptionsName = name }, name ?? string.Empty);

        pipeline.Execute(_ => "dummy", ctx);
        ctx.Properties.GetValue(TagKey, string.Empty).ShouldBe("reloaded-tag");

        resList.Count.ShouldBe(2);
        resList[0].Received(1).Dispose();
        resList[1].Received(0).Dispose();

        serviceProvider.Dispose();
        resList[0].Received(1).Dispose();
        resList[1].Received(1).Dispose();
    }

    [Fact]
    public void EnableReloadsWithMonitor_NullMonitor_Throws()
    {
        var called = false;
        var services = new ServiceCollection();
        services.AddResiliencePipeline("my-pipeline", (_, context) =>
        {
            called = true;
            Assert.Throws<ArgumentNullException>("monitor",
                () => context.EnableReloadsWithMonitor((IOptionsMonitor<ReloadableStrategyOptions>)null!));
        });

        services.BuildServiceProvider()
            .GetRequiredService<ResiliencePipelineProvider<string>>()
            .GetPipeline("my-pipeline");

        called.ShouldBeTrue();
    }

    public class ReloadableStrategy(string tag, IDisposable disposableResource) : ResilienceStrategy, IDisposable
    {
        public string Tag { get; } = tag;

        public IDisposable DisposableResource { get; } = disposableResource;

        public void Dispose() => DisposableResource.Dispose();

        protected override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            context.Properties.Set(TagKey, Tag);
            return callback(context, state);
        }
    }

    public class ReloadableStrategyOptions : ResilienceStrategyOptions
    {
        public string Tag { get; set; } = string.Empty;

        public string? OptionsName { get; set; }
    }

    private class ReloadableConfiguration : ConfigurationProvider, IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => this;

        public void Reload(Dictionary<string, string?> data)
        {
            Data = new Dictionary<string, string?>(data, StringComparer.OrdinalIgnoreCase);
            OnReload();
        }
    }

    private sealed class FakeOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>
    {
        private readonly List<Action<TOptions, string?>> _listeners = [];

        public FakeOptionsMonitor(TOptions initialValue) => CurrentValue = initialValue;

        public TOptions CurrentValue { get; private set; }

        public TOptions Get(string? name) => CurrentValue;

        public IDisposable? OnChange(Action<TOptions, string?> listener)
        {
            _listeners.Add(listener);
            return new CallbackDisposable(() => _listeners.Remove(listener));
        }

        public void TriggerChange(TOptions newValue, string? name = null)
        {
            CurrentValue = newValue;
            foreach (var listener in _listeners.ToList())
            {
                listener(newValue, name);
            }
        }

        private sealed class CallbackDisposable(Action callback) : IDisposable
        {
            public void Dispose() => callback();
        }
    }
}
