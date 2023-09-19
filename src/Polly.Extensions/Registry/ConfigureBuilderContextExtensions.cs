using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.Options;
using Polly.Utils;

namespace Polly.Registry;

/// <summary>
/// Extensions for <see cref="ConfigureBuilderContext{TKey}"/>.
/// </summary>
public static class ConfigureBuilderContextExtensions
{
    /// <summary>
    /// Enables dynamic reloading of the resilience pipeline whenever the <typeparamref name="TOptions"/> options are changed.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience pipeline.</typeparam>
    /// <typeparam name="TOptions">The options type to listen to.</typeparam>
    /// <param name="context">The builder context.</param>
    /// <param name="optionsMonitor">The options monitor.</param>
    /// <param name="name">The named options, if any.</param>
    /// <remarks>
    /// You can decide based on the <paramref name="name"/> to listen for changes in global options or named options.
    /// If <paramref name="name"/> is <see langword="null"/> then the global options are listened to.
    /// <para>
    /// You can listen for changes from multiple options by calling this method with different <typeparamref name="TOptions"/> types.
    /// </para>
    /// </remarks>
    public static void EnableReloads<TKey, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>(
        this ConfigureBuilderContext<TKey> context,
        IOptionsMonitor<TOptions> optionsMonitor,
        string? name = null)
        where TKey : notnull
    {
        Guard.NotNull(context);
        Guard.NotNull(optionsMonitor);

        name ??= string.Empty;

#pragma warning disable CA2000 // Dispose objects before losing scope
        var source = new CancellationTokenSource();
#pragma warning restore CA2000 // Dispose objects before losing scope
        var registration = optionsMonitor.OnChange((_, changedNamed) =>
        {
            if (name == changedNamed)
            {
                source.Cancel();
            }
        });

        context.AddReloadToken(source.Token);
        context.OnPipelineDisposed(() =>
        {
            registration?.Dispose();
            source.Dispose();
        });
    }
}
