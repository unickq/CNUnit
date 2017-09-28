using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;

[assembly: AssemblyTitle("CNUnit.Test")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("CNUnit.Test")]
[assembly: AssemblyCopyright("Nick Chursin")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: Guid("c2fdf51a-e6be-4fb6-b19a-244711d9bb64")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
#if !DEBUG
[assembly: Parallelizable(ParallelScope.None)]
#else
[assembly: LevelOfParallelism(4)]
[assembly: Parallelizable(ParallelScope.Fixtures)]
#endif