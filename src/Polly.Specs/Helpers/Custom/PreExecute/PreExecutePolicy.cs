﻿namespace Polly.Specs.Helpers.Custom.PreExecute;

internal class PreExecutePolicy : Policy
{
    private readonly Action _preExecute;

    public static PreExecutePolicy Create(Action preExecute) => new(preExecute);

    internal PreExecutePolicy(Action preExecute) =>
        _preExecute = preExecute ?? throw new ArgumentNullException(nameof(preExecute));

    protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken) =>
        PreExecuteEngine.Implementation(_preExecute, action, context, cancellationToken);
}

internal class PreExecutePolicy<TResult> : Policy<TResult>
{
    private readonly Action _preExecute;

    public static PreExecutePolicy<TResult> Create(Action preExecute) => new(preExecute);

    internal PreExecutePolicy(Action preExecute) =>
        _preExecute = preExecute ?? throw new ArgumentNullException(nameof(preExecute));

    protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken) =>
        PreExecuteEngine.Implementation(_preExecute, action, context, cancellationToken);
}
