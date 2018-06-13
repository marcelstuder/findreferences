using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Serilog;

namespace FindAllReferences
{
    internal class Program
    {
        private static void Main()
        {
            using (var log = new LoggerConfiguration().WriteTo.Console().CreateLogger())
            {
                FindReferences(log: log,
                    pathToSolution: @"../../FindAllReferences.sln",
                    projectName: "FindAllReferences",
                    nspace: "FindAllReferences.CodeToAnalyze",
                    classToAnalyze: "SomeEntity");

                Console.WriteLine("Press any key to continue...");
                Console.ReadLine();
            }
        }

        private static void FindReferences(ILogger log, string pathToSolution, string projectName, string nspace, string classToAnalyze)
        {
            using (var msWorkspace = MSBuildWorkspace.Create())
            {
                msWorkspace.WorkspaceFailed += (sender, args) => log.Error(args.Diagnostic.Message);
                var solution = msWorkspace.OpenSolutionAsync(pathToSolution).GetAwaiter().GetResult();
                // look for the project that contains the class to analyze
                var project = solution.Projects.Single(p => p.Name == projectName);
                // get the class file
                var doc = project.Documents.Single(d => d.Name == $"{classToAnalyze}.cs");
                
                // get the semantic model and lookup the type
                var semanticModel = doc.GetSemanticModelAsync().GetAwaiter().GetResult();
                var someEntityType = semanticModel.Compilation.GetTypeByMetadataName($"{nspace}.{classToAnalyze}");

                // get all public properties of the entity to analyze
                var properties = someEntityType.GetMembers()
                    .Where(m => m.Kind == SymbolKind.Property)
                    .Where(p => p.DeclaredAccessibility == Accessibility.Public);

                log.Information($"Class: {classToAnalyze}");

                foreach (var prop in properties)
                {
                    // check the whole solution for usages of the current property
                    var references = SymbolFinder.FindReferencesAsync(prop, solution).GetAwaiter()
                        .GetResult()
                        .Where(r => r.Definition.Kind == SymbolKind.Property)
                        .ToList();

                    var re = references.FirstOrDefault();

                    if (null == re)
                    {
                        log.Warning($"\tNo property reference found for property '{prop.Name}'. Property skipped.");
                        continue;
                    }

                    // currently we don't handle multiple references with multiple locations
                    // (i.e. for virtual properties inherited from a base class)
                    if (references.Count > 1)
                    {
                        log.Warning($"\tMore than 1 property reference found for property '{prop.Name}': {references.Count}");
                    }
                    else
                    {
                        var message = $"\t{prop.Name}: {re.Locations.Count()} references.";
                        if (re.Locations.Count() > 1)
                            log.Information(message);
                        else
                            log.Warning(message);
                    }
                }
            }
        }
    }
}
