using System.Runtime.CompilerServices;
using Polly;

#pragma warning disable RS0016

[assembly: TypeForwardedTo(typeof(ExecutionRejectedException))]
[assembly: TypeForwardedTo(typeof(TimeoutRejectedException))]
[assembly: TypeForwardedTo(typeof(BrokenCircuitException))]
[assembly: TypeForwardedTo(typeof(BrokenCircuitException<>))]
[assembly: TypeForwardedTo(typeof(IsolatedCircuitException))]
[assembly: TypeForwardedTo(typeof(CircuitState))]
