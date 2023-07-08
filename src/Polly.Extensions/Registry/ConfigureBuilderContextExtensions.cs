﻿using Microsoft.Extensions.Options;
using Polly.Extensions.Utils;
using Polly.Registry;
using Polly.Utils;

namespace Polly.Extensions.Registry;

/// <summary>
/// Extensions for <see cref="ConfigureBuilderContext{TKey}"/>.
/// </summary>
public static class ConfigureBuilderContextExtensions
{
    /// <summary>
    /// Enables dynamic reloading of the resilience strategy whenever the <typeparamref name="TOptions"/> options are changed.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience strategy.</typeparam>
    /// <typeparam name="TOptions">The options type to listen to.</typeparam>
    /// <param name="context">The builder context.</param>
    /// <param name="optionsMonitor">The options monitor.</param>
    /// <param name="name">The named options, if any.</param>
    /// <remarks>
    /// You can decide based on the <paramref name="name"/> to listen for changes in global options or named options.
    /// If <paramref name="name"/> is <see langword="null"/> then the global options are listened to.
    /// <para>
    /// You can listen for changes only for single options. If you call this method multiple times, the preceding calls are ignored and only the last one wins.
    /// </para>
    /// </remarks>
    public static void EnableReloads<TKey, TOptions>(this ConfigureBuilderContext<TKey> context, IOptionsMonitor<TOptions> optionsMonitor, string? name = null)
        where TKey : notnull
    {
        Guard.NotNull(context);
        Guard.NotNull(optionsMonitor);

        context.EnableReloads(() => new OptionsReloadHelper<TOptions>(optionsMonitor, name ?? Options.DefaultName).GetCancellationToken);
    }
}
