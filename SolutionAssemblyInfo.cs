using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyProduct("Polly")]
[assembly: AssemblyCompany("Michael Wolfenden")]
[assembly: AssemblyCopyright("Copyright © 2013, Michael Wolfenden")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]