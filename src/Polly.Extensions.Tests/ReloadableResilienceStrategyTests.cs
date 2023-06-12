using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Polly.Extensions.Utils;
using Polly.Registry;

namespace Polly.Extensions.Tests;
public class ReloadableResilienceStrategyTests
{
    private static readonly ResiliencePropertyKey<string> TagKey = new("tests.tag");

    [InlineData(null)]
    [InlineData("custom-name")]
    [InlineData("")]
    [Theory]
    public void AddResilienceStrategy_EnsureReloadable(string? name)
    {
        var reloadableConfig = new ReloadableConfiguration();
        reloadableConfig.Reload(new() { { "tag", "initial-tag" } });
        var builder = new ConfigurationBuilder().Add(reloadableConfig);

        var services = new ServiceCollection();

        if (name == null)
        {
            services.Configure<ReloadableStrategyOptions>(builder.Build());
        }
        else
        {
            services.Configure<ReloadableStrategyOptions>(name, builder.Build());
        }

        services.AddResilienceStrategy("my-strategy", (builder, context) =>
        {
            var options = context.GetOptions<ReloadableStrategyOptions>(name);
            context.EnableReloads<ReloadableStrategyOptions>(name);

            builder.AddStrategy(_ => new ReloadableStrategy(options.Tag), new ReloadableStrategyOptions());
        });

        var serviceProvider = services.BuildServiceProvider();
        var strategy = serviceProvider.GetRequiredService<ResilienceStrategyProvider<string>>().Get("my-strategy");
        var context = ResilienceContext.Get();
        var registry = serviceProvider.GetRequiredService<OptionsReloadHelperRegistry<string>>();

        // initial
        strategy.Execute(_ => "dummy", context);
        context.Properties.GetValue(TagKey, string.Empty).Should().Be("initial-tag");

        // reloads
        for (int i = 0; i < 10; i++)
        {
            reloadableConfig.Reload(new() { { "tag", $"reload-{i}" } });
            strategy.Execute(_ => "dummy", context);
            context.Properties.GetValue(TagKey, string.Empty).Should().Be($"reload-{i}");
        }

        registry.Count.Should().Be(1);
        serviceProvider.Dispose();
        registry.Count.Should().Be(0);
    }

    [Fact]
    public void OptionsReloadHelperRegistry_EnsureTracksDifferentHelpers()
    {
        var services = new ServiceCollection().AddResilienceStrategy("dummy", builder => { });
        var serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetRequiredService<OptionsReloadHelperRegistry<string>>();

        registry.Get<ReloadableStrategy>("A", null);
        registry.Get<ReloadableStrategy>("A", "dummy");
        registry.Get<ReloadableStrategy>("B", null);
        registry.Get<ReloadableStrategy>("B", "dummy");

        registry.Count.Should().Be(4);

        registry.Dispose();
        registry.Count.Should().Be(0);
    }

    [Fact]
    public void OptionsReloadHelper_Dispose_Ok()
    {
        var monitor = new Mock<IOptionsMonitor<ReloadableStrategyOptions>>();

        using var helper = new OptionsReloadHelper<ReloadableStrategyOptions>(monitor.Object, null);

        helper.Invoking(h => h.Dispose()).Should().NotThrow();
    }

    public class ReloadableStrategy : ResilienceStrategy
    {
        public ReloadableStrategy(string tag) => Tag = tag;

        public string Tag { get; }

        protected override ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
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
        public override string StrategyType => "Reloadable";

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
