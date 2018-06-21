## Problem description

We are trying to find all references to public properties of automatically generated entities. Those entities are mapped using Fluent NHibernate and some of the properties are being used in repositories. In order to remove properties that are only mapped but nowhere used we would like to use roslyn to find out entities with properties having a reference count < 2 (i.e. not mapped properties or properties mapped using nhibernate but not used anywhere else in the solution).

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

## Fix for error: could not load assembly 'Microsoft.Build.Utilities.Core'

Based on this [github-issue](https://github.com/dotnet/roslyn/issues/19978) we have added the following bindingredirects:

```
      <!-- added bindingredirects according to https://github.com/dotnet/roslyn/issues/19978 -->
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.Build" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="14.0.0.0" newVersion="15.1.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.Build.Conversion.Core" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="14.0.0.0" newVersion="15.1.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.Build.Engine" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="14.0.0.0" newVersion="15.1.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.Build.Framework" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="14.0.0.0" newVersion="15.1.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.Build.Tasks.Core" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="14.0.0.0" newVersion="15.1.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.Build.Utilities.Core" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="14.0.0.0" newVersion="15.1.0.0" />
```

This resolved one of the two errors but the problem with the wrong reference count (as long as the class library project is referenced) remains.

## Environment details

```
Microsoft Visual Studio Enterprise 2017 
Version 15.7.3
VisualStudio.15.Release/15.7.3+27703.2026
Microsoft .NET Framework
Version 4.7.03056

Installed Version: Enterprise

Application Insights Tools for Visual Studio Package   8.12.10405.1
Application Insights Tools for Visual Studio

ASP.NET and Web Tools 2017   15.0.40522.0
ASP.NET and Web Tools 2017

ASP.NET Core Razor Language Services   15.7.31476
Provides languages services for ASP.NET Core Razor.

ASP.NET Web Frameworks and Tools 2017   5.2.60419.0
For additional information, visit https://www.asp.net/

Azure App Service Tools v3.0.0   15.0.40424.0
Azure App Service Tools v3.0.0

Azure Data Lake Node   1.0
This package contains the Data Lake integration nodes for Server Explorer.

Azure Data Lake Tools for Visual Studio   2.3.3000.5
Microsoft Azure Data Lake Tools for Visual Studio

Azure Functions and Web Jobs Tools   15.0.40502.0
Azure Functions and Web Jobs Tools

Azure Stream Analytics Tools for Visual Studio   2.3.3000.5
Microsoft Azure Stream Analytics Tools for Visual Studio

C# Tools   2.8.3-beta6-62923-07. Commit Hash: 7aafab561e449da50712e16c9e81742b8e7a2969
C# components used in the IDE. Depending on your project type and settings, a different version of the compiler may be used.

Common Azure Tools   1.10
Provides common services for use by Azure Mobile Services and Microsoft Azure Tools.

Cookiecutter   15.7.18131.1
Provides tools for finding, instantiating and customizing templates in cookiecutter format.

Fabric.ApplicationInsights   1.0
Allows user to instrument their Service Fabric projects for Application Insights.

Fabric.DiagnosticEvents   1.0
Fabric Diagnostic Events

JavaScript Language Service   2.0
JavaScript Language Service

JavaScript Project System   2.0
JavaScript Project System

Merq   1.1.19-rc (a4ffc1b)
Command Bus, Event Stream and Async Manager for Visual Studio extensions.

Microsoft Azure HDInsight Azure Node   2.3.3000.5
HDInsight Node under Azure Node

Microsoft Azure Hive Query Language Service   2.3.3000.5
Language service for Hive query

Microsoft Azure Service Fabric Tools for Visual Studio   2.1
Microsoft Azure Service Fabric Tools for Visual Studio

Microsoft Azure Stream Analytics Language Service   2.3.3000.5
Language service for Azure Stream Analytics

Microsoft Azure Stream Analytics Node   1.0
Azure Stream Analytics Node under Azure Node

Microsoft Azure Tools   2.9
Microsoft Azure Tools for Microsoft Visual Studio 2017 - v2.9.10420.2

Microsoft Continuous Delivery Tools for Visual Studio   0.3
Simplifying the configuration of continuous build integration and continuous build delivery from within the Visual Studio IDE.

Microsoft JVM Debugger   1.0
Provides support for connecting the Visual Studio debugger to JDWP compatible Java Virtual Machines

Microsoft MI-Based Debugger   1.0
Provides support for connecting Visual Studio to MI compatible debuggers

Microsoft Visual Studio Tools for Containers   1.1
Develop, run, validate your ASP.NET Core applications in the target environment. F5 your application directly into a container with debugging, or CTRL + F5 to edit & refresh your app without having to rebuild the container.

Mono Debugging for Visual Studio   4.10.5-pre (ab58725)
Support for debugging Mono processes with Visual Studio.

Node.js Tools   1.4.11027.3
Adds support for developing and debugging Node.js apps in Visual Studio

NuGet Package Manager   4.6.0
NuGet Package Manager in Visual Studio. For more information about NuGet, visit http://docs.nuget.org/.

Office Developer Tools for Visual Studio 2017 ENU   15.0.27612.00
Microsoft Office Developer Tools for Visual Studio 2017 ENU

ProjectServicesPackage Extension   1.0
ProjectServicesPackage Visual Studio Extension Detailed Info

Python   15.7.18131.1
Provides IntelliSense, projects, templates, debugging, interactive windows, and other support for Python developers.

Python - Django support   15.7.18131.1
Provides templates and integration for the Django web framework.

Python - IronPython support   15.7.18131.1
Provides templates and integration for IronPython-based projects.

Python - Profiling support   15.7.18131.1
Profiling support for Python projects.

R Tools for Visual Studio   1.3.40517.1016
Provides project system, R Interactive window, plotting, and more for the R programming language.

Redgate ReadyRoll   1.17.18158.17
Extend DevOps processes to your SQL Server databases and safely automate database deployments.

Visit https://www.red-gate.com/readyroll for more information.

Copyright (C) 2011 Red Gate Software Ltd. All rights reserved.

This software contains components from Component Owl.
SQL Server is a registered trademark of Microsoft Corporation.
Visual Studio is a registered trademark of Microsoft Corporation.

ReadyRoll contains code from the following open source software:

NuGet https://www.nuget.org/
SQL LocalDB Wrapper https://github.com/martincostello/sqllocaldb
Autofac https://autofac.org/
Json.NET https://json.net/
MahApps.Metro http://mahapps.com/
SemVer https://github.com/maxhauser/semver
Log4Net http://logging.apache.org/log4net/
StringTemplate https://github.com/antlr/stringtemplate4
Extended WPF Toolkit https://wpftoolkit.codeplex.com/
Code InfoBox VSX http://www.codeproject.com/Articles/55196/Code-InfoBox-Visual-Studio-Extension-VSX
OctoPack https://github.com/OctopusDeploy/OctoPack
SQLite https://sqlite.org/

This product contains icons from http://www.visualpharm.com distributed under a free backlink license.

For license details or other notices relating to the above software, please see NOTICE.TXT and EULA.rtf in the ReadyRoll application folder.
    

Redgate SQL Prompt   9.1.15.5260
Write, format, and refactor SQL effortlessly

ResourcePackage Extension   1.0
ResourcePackage Visual Studio Extension Detailed Info

Snapshot Debugging Extension   1.0
Snapshot Debugging Visual Studio Extension Detailed Info

SQL Server Data Tools   15.1.61804.210
Microsoft SQL Server Data Tools

ToolWindowHostedEditor   1.0
Hosting json editor into a tool window

TypeScript Tools   15.7.20419.2003
TypeScript Tools for Microsoft Visual Studio

Visual Basic Tools   2.8.3-beta6-62923-07. Commit Hash: 7aafab561e449da50712e16c9e81742b8e7a2969
Visual Basic components used in the IDE. Depending on your project type and settings, a different version of the compiler may be used.

Visual F# Tools 10.1 for F# 4.1   15.7.0.0.  Commit Hash: 2527e6829ecdc8281ee60d83be8cfd0fa720a648.
Microsoft Visual F# Tools 10.1 for F# 4.1

Visual Studio Code Debug Adapter Host Package   1.0
Interop layer for hosting Visual Studio Code debug adapters in Visual Studio

Visual Studio Tools for Apache Cordova   15.123.7408.1
Visual Studio Tools for Apache Cordova

VisualStudio.Mac   1.0
Mac Extension for Visual Studio

Workflow Manager Tools 1.0   1.0
This package contains the necessary Visual Studio integration components for Workflow Manager.

Xamarin   4.10.10.1 (f1760154c)
Visual Studio extension to enable development for Xamarin.iOS and Xamarin.Android.

Xamarin Designer   4.12.1 (f3257e429)
Visual Studio extension to enable Xamarin Designer tools in Visual Studio.

Xamarin.Android SDK   8.3.3.2 (HEAD/dffc59120)
Xamarin.Android Reference Assemblies and MSBuild support.

Xamarin.iOS and Xamarin.Mac SDK   11.12.0.4 (64fece5)
Xamarin.iOS and Xamarin.Mac Reference Assemblies and MSBuild support.

```

## All issues resolved

Based on these hints (https://github.com/dotnet/roslyn/issues/24691) all issues have been resolved.
