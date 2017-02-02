using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Polly")]
[assembly: CLSCompliant(false)] // Because Nito.AsycEx, on which Polly.Net40Async depends, is not CLSCompliant.

[assembly: InternalsVisibleTo("Polly.Net40Async.Specs")]