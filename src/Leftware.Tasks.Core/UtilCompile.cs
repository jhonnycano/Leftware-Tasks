using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;
using System.Runtime.Loader;

namespace Leftware.Tasks.Core;

/// <summary>
/// 
/// https://stackoverflow.com/questions/71474900/dynamic-compilation-in-net-core-6
/// </summary>
public class UtilCompile
{
    public static (Type? type, IList<string> errors) Compile(string sourceCode)
    {
        var sourceText = SourceText.From(sourceCode);
        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10);
        var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText, options);
        var references = new List<MetadataReference>
    {
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
    };
        //var assemblyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Leftware", "Temp", "dynamicAssembly.dll");
        var assemblyPath = "dynamicAssembly.dll";
        var csCompilation = CSharpCompilation.Create(assemblyPath,
            new[] { parsedSyntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication,
            optimizationLevel: OptimizationLevel.Release,
            assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

        //WeakReference? assemblyLoadContextWeakRef = null;
        using (var peStream = new MemoryStream())
        {
            var result = csCompilation.Emit(peStream);

            if (result.Success)
            {
                Console.WriteLine("Compilation done without any error.");
                peStream.Seek(0, SeekOrigin.Begin);
                var compiledAssembly = peStream.ToArray();

                using (var asm = new MemoryStream(compiledAssembly))
                {
                    var assemblyLoadContext = new SimpleAssemblyLoadContext();
                    var assembly = assemblyLoadContext.LoadFromStream(asm);
                    var type = assembly.GetType("DynamicAssembly.CustomFilters");
                    if (type is null) return (null, new List<string> { "Type DynamicAssembly.CustomFilters not found in dynamic code" });
                    return (type, new List<string>());

                    //assemblyLoadContext.Unload();
                    //assemblyLoadContextWeakRef = new WeakReference(assemblyLoadContext);
                }
            }
            else
            {
                Console.WriteLine("Compilation done with error.");
                var failures = result.Diagnostics
                    .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                    .Select(diagnostic => $"{diagnostic.Id}: {diagnostic.GetMessage()}")
                    .ToList();
                return (null, failures);
            }
        }
    }
}

internal class SimpleAssemblyLoadContext : AssemblyLoadContext
{
    public SimpleAssemblyLoadContext()
        : base(true)
    {
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        return null;
    }
}