// Assembly 'Polly.Core'

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Polly.Telemetry;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed record TelemetryEventArguments(ResilienceTelemetrySource Source, string EventName, ResilienceContext Context, Outcome<object>? Outcome, object Arguments);
