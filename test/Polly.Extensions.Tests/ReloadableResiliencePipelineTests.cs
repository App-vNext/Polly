using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Polly.DependencyInjection;
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
        var builder = new ConfigurationBuilder().Add(reloadableConfig);
        var fakeListener = new FakeTelemetryListener();

        var services = new ServiceCollection();

        if (name == null)
        {
            services.Configure<ReloadableStrategyOptions>(builder.Build());
        }
        else
        {
            services.Configure<ReloadableStrategyOptions>(name, builder.Build());
        }

        services.Configure<TelemetryOptions>(options => options.TelemetryListeners.Add(fakeListener));
        services.AddResiliencePipeline("my-pipeline", (builder, context) =>
        {
            builder.InstanceName = "my-instance";

            var options = context.GetOptions<ReloadableStrategyOptions>(name);
            context.EnableReloads<ReloadableStrategyOptions>(name);

            builder.AddStrategy(_ =>
            {
                var res = Substitute.For<IDisposable>();
                resList.Add(res);
                return new ReloadableStrategy(options.Tag, res);
            },
            new ReloadableStrategyOptions());
        });

        var serviceProvider = services.BuildServiceProvider();
        var pipeline = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>().GetPipeline("my-pipeline");
        var context = ResilienceContextPool.Shared.Get();

        // initial
        pipeline.Execute(_ => "dummy", context);
        context.Properties.GetValue(TagKey, string.Empty).Should().Be("initial-tag");

        // reloads
        for (int i = 0; i < 10; i++)
        {
            reloadableConfig.Reload(new() { { "tag", $"reload-{i}" } });
            pipeline.Execute(_ => "dummy", context);
            context.Properties.GetValue(TagKey, string.Empty).Should().Be($"reload-{i}");
        }

        // check resource disposed
        resList.Should().HaveCount(11);
        for (int i = 0; i < resList.Count - 1; i++)
        {
            resList[i].Received(1).Dispose();
        }

        resList[resList.Count - 1].Received(0).Dispose();

        // check disposal of service provider
        serviceProvider.Dispose();
        resList[resList.Count - 1].Received(1).Dispose();
        pipeline.Invoking(p => p.Execute(() => { })).Should().Throw<ObjectDisposedException>();

        foreach (var ev in fakeListener.Events)
        {
            ev.Source.PipelineName.Should().Be("my-pipeline");
            ev.Source.PipelineInstanceName.Should().Be("my-instance");
        }
    }

    public class ReloadableStrategy : ResilienceStrategy, IDisposable
    {
        public ReloadableStrategy(string tag, IDisposable disposableResource)
        {
            Tag = tag;
            DisposableResource = disposableResource;
        }

        public string Tag { get; }

        public IDisposable DisposableResource { get; }

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
    }

    private class ReloadableConfiguration : ConfigurationProvider, IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }

        public void Reload(Dictionary<string, string?> data)
        {
            Data = new Dictionary<string, string?>(data, StringComparer.OrdinalIgnoreCase);
            OnReload();
        }
    }
}
