using System.ComponentModel;

namespace Polly.Registry;

/// <summary>
/// The context used by <see cref="ResiliencePipelineRegistry{TKey}"/>.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public class ConfigureBuilderContext<TKey>
    where TKey : notnull
{
    internal ConfigureBuilderContext(TKey strategyKey, string builderName, string? builderInstanceName)
    {
        PipelineKey = strategyKey;
        BuilderName = builderName;
        BuilderInstanceName = builderInstanceName;
    }

    /// <summary>
    /// Gets the pipeline key for the pipeline being created.
    /// </summary>
    public TKey PipelineKey { get; }

    /// <summary>
    /// Gets the builder name for the builder being used to create the strategy.
    /// </summary>
    internal string BuilderName { get; }

    /// <summary>
    /// Gets the instance name for the builder being used to create the strategy.
    /// </summary>
    internal string? BuilderInstanceName { get; }

    internal List<CancellationToken> ReloadTokens { get; } = new();

    internal List<Action> DisposeCallbacks { get; } = new();

    /// <summary>
    /// Reloads the pipeline when <paramref name="cancellationToken"/> is canceled.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that triggers the pipeline reload when cancelled.</param>
    /// <remarks>
    /// You can add multiple reload tokens to the context. Non-cancelable or already canceled tokens are ignored.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AddReloadToken(CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled || cancellationToken.IsCancellationRequested)
        {
            return;
        }

        ReloadTokens.Add(cancellationToken);
    }

    /// <summary>
    /// Registers a callback that is called when the pipeline instance being configured is disposed.
    /// </summary>
    /// <param name="callback">The callback delegate.</param>
    public void OnPipelineDisposed(Action callback)
    {
        Guard.NotNull(callback);

        DisposeCallbacks.Add(callback);
    }
}
