using System.Reflection;

[assembly: AssemblyProduct("Polly")]
[assembly: AssemblyCompany("App vNext")]
[assembly: AssemblyDescription("Polly is a library that allows developers to express transient exception handling policies such as Retry, Retry Forever, Wait and Retry or Circuit Breaker in a fluent manner.")]
[assembly: AssemblyCopyright("Copyright (c) 2016, App vNext")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
