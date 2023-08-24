// Assembly 'Polly.Core'

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Polly.Registry;

public class ConfigureBuilderContext<TKey> where TKey : notnull
{
    public TKey PipelineKey { get; }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AddReloadToken(CancellationToken cancellationToken);
    public void OnPipelineDisposed(Action callback);
}
