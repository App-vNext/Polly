// Assembly 'Polly.Core'

using System;

namespace Polly;

public abstract class ExecutionRejectedException : Exception
{
    protected ExecutionRejectedException();
    protected ExecutionRejectedException(string message);
    protected ExecutionRejectedException(string message, Exception inner);
}
