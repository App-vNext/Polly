// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Timeout;

public class TimeoutRejectedException : ExecutionRejectedException
{
    public TimeSpan Timeout { get; }
    public TimeoutRejectedException();
    public TimeoutRejectedException(string message);
    public TimeoutRejectedException(string message, Exception innerException);
    public TimeoutRejectedException(TimeSpan timeout);
    public TimeoutRejectedException(string message, TimeSpan timeout);
    public TimeoutRejectedException(string message, TimeSpan timeout, Exception innerException);
}
