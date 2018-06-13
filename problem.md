## Problem description

We are trying to find all references to public properties of automatically generated entities. Those entities are mapped using Fluent NHibernate and some of the properties are being used in repositories. In order to remove properties that are only mapped but nowhere used we would like to use roslyn to find out entities with properties having a reference count < 2 (which is not mapped properties or properties mapped using nhibernate not used anywhere in the solution).

The FindAllReferences console application demonstrates our initial approach to finding references on the following class:

```
    public class SomeEntity 
    {
        // should have a reference cound of 0 - see Visual Studio
        public virtual string PropertyNotMapped { get; set; }
        // should have a reference count of 1 - see Visual Studio
        public virtual string PropertyMappedButNotReferenced { get; set; }
        // should have a reference count of 2 - see Visual Studio
        public virtual string PropertyMappedAndReferenced { get; set; }
    }
```

We are having multiple issues (that are probably related):
* we get som MsBuild errors when analyzing the solution using MsBuildWorkspace
* we get the correct reference count when we do not reference the class library **SomeClassLibrary**
* we get incorrect reference counts when we reference the class library **SomeClassLibrary** (probably caused by the errors indicated by Msbuild)

## Error (bad reference count)

When the **FindAllReferences** console app has a project reference to **SomeClassLibrary** then the reference counts are wrong (and we have some Msbuild errors).

```
[18:17:08 ERR] Msbuild failed when processing the file 'C:\work\kaseyu\FindAllReferences\FindAllReferences.csproj' with message: C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\Microsoft.Common.CurrentVersion.targets: (1656, 5): The "GetReferenceNearestTargetFrameworkTask" task could not be instantiated from the assembly "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\CommonExtensions\Microsoft\NuGet\NuGet.Build.Tasks.dll". Please verify the task assembly has been built using the same version of the Microsoft.Build.Framework assembly as the one installed on your computer and that your host application is not missing a binding redirect for Microsoft.Build.Framework. Unable to cast object of type 'NuGet.Build.Tasks.GetReferenceNearestTargetFrameworkTask' to type 'Microsoft.Build.Framework.ITask'.
C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\Microsoft.Common.CurrentVersion.targets: (1656, 5): The "GetReferenceNearestTargetFrameworkTask" task has been declared or used incorrectly, or failed during construction. Check the spelling of the task name and the assembly name.
[18:17:08 ERR] Msbuild failed when processing the file 'C:\work\kaseyu\SomeClassLibrary\SomeClassLibrary.csproj' with message: C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\Roslyn\Microsoft.CSharp.Core.targets: (52, 5): The "Microsoft.CodeAnalysis.BuildTasks.Csc" task could not be loaded from the assembly C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\Roslyn\Microsoft.Build.Tasks.CodeAnalysis.dll. Could not load file or assembly 'Microsoft.Build.Utilities.Core, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies. The system cannot find the file specified. Confirm that the <UsingTask> declaration is correct, that the assembly and all its dependencies are available, and that the task contains a public class that implements Microsoft.Build.Framework.ITask.
[18:17:10 INF] Class: SomeEntity
[18:17:10 WRN]  PropertyNotMapped: 0 references.
[18:17:11 WRN]  PropertyMappedButNotReferenced: `0 references`.
[18:17:11 WRN]  PropertyMappedAndReferenced: `1 references`.
Press any key to continue...
```

## Correct reference count

As soon as the reference to the ClassLibrary **SomeClassLibrary** is removed (it isn't in use anyway) the reference counts are correct (although the msubild errors remain).

```
[18:21:33 ERR] Msbuild failed when processing the file 'C:\work\kaseyu\FindAllReferences\FindAllReferences.csproj' with message: C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\Roslyn\Microsoft.CSharp.Core.targets: (52, 5): The "Microsoft.CodeAnalysis.BuildTasks.Csc" task could not be loaded from the assembly C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\Roslyn\Microsoft.Build.Tasks.CodeAnalysis.dll. Could not load file or assembly 'Microsoft.Build.Utilities.Core, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies. The system cannot find the file specified. Confirm that the <UsingTask> declaration is correct, that the assembly and all its dependencies are available, and that the task contains a public class that implements Microsoft.Build.Framework.ITask.
[18:21:33 ERR] Msbuild failed when processing the file 'C:\work\kaseyu\SomeClassLibrary\SomeClassLibrary.csproj' with message: C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\Roslyn\Microsoft.CSharp.Core.targets: (52, 5): The "Microsoft.CodeAnalysis.BuildTasks.Csc" task could not be loaded from the assembly C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\Roslyn\Microsoft.Build.Tasks.CodeAnalysis.dll. Could not load file or assembly 'Microsoft.Build.Utilities.Core, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies. The system cannot find the file specified. Confirm that the <UsingTask> declaration is correct, that the assembly and all its dependencies are available, and that the task contains a public class that implements Microsoft.Build.Framework.ITask.
[18:21:34 INF] Class: SomeEntity
[18:21:35 WRN]  PropertyNotMapped: 0 references.
[18:21:35 WRN]  PropertyMappedButNotReferenced: 1 references.
[18:21:35 INF]  PropertyMappedAndReferenced: 2 references.
Press any key to continue...
``` 