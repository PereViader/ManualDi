
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using ManualDi.Async.Generators;

namespace ManualDi.Async.Tests
{
    public static class GeneratorTestHelper
    {
        public static (IEnumerable<string> code, ImmutableArray<Diagnostic> diagnostics) Generate(string code)
        {
            var references = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location) && !a.Location.Contains("ManualDi.Async.Tests.dll"))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();

            references.Add(MetadataReference.CreateFromFile(typeof(IDiContainer).Assembly.Location));

            var compilation = CSharpCompilation.Create("AssemblyName",
                [CSharpSyntaxTree.ParseText(SourceText.From(code, Encoding.UTF8))],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithNullableContextOptions(NullableContextOptions.Enable));

            var driver = CSharpGeneratorDriver.Create(new ManualDiSourceGenerator());
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            var generatedTrees = outputCompilation.SyntaxTrees.ToList();

            var generatedCode = generatedTrees.Skip(1).Select(x => x.ToString());
            return (code: generatedCode, diagnostics: outputCompilation.GetDiagnostics());
        }
    }
}
