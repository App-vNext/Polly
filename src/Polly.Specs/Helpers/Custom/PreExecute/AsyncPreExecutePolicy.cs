﻿namespace Polly.Specs.Helpers.Custom.PreExecute;

internal class AsyncPreExecutePolicy : AsyncPolicy
{
    private readonly Func<Task> _preExecute;

    public static AsyncPreExecutePolicy CreateAsync(Func<Task> preExecute) => new(preExecute);

    internal AsyncPreExecutePolicy(Func<Task> preExecute) =>
        _preExecute = preExecute ?? throw new ArgumentNullException(nameof(preExecute));

    protected override Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
        bool continueOnCapturedContext) =>
        AsyncPreExecuteEngine.ImplementationAsync(_preExecute, action, context, cancellationToken, continueOnCapturedContext);
}

internal class AsyncPreExecutePolicy<TResult> : AsyncPolicy<TResult>
{
    private readonly Func<Task> _preExecute;

    public static AsyncPreExecutePolicy<TResult> CreateAsync(Func<Task> preExecute) =>
        new(preExecute);

    internal AsyncPreExecutePolicy(Func<Task> preExecute) =>
        _preExecute = preExecute ?? throw new ArgumentNullException(nameof(preExecute));

    protected override Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
        bool continueOnCapturedContext) =>
        AsyncPreExecuteEngine.ImplementationAsync(_preExecute, action, context, cancellationToken, continueOnCapturedContext);
}
