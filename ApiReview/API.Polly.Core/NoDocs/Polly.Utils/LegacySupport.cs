// Assembly 'Polly.Core'

using System.Collections.Generic;
using System.ComponentModel;

namespace Polly.Utils;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class LegacySupport
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetProperties(this ResilienceProperties resilienceProperties, IDictionary<string, object?> properties, out IDictionary<string, object?> oldProperties);
}
