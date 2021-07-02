using System.Reflection;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run(Assembly.GetCallingAssembly(), args: args); 
