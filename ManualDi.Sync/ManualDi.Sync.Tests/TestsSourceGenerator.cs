using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using ManualDi.Sync.Generators;
using System.Linq;
using NUnit.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Threading.Tasks;
using VerifyNUnit;

namespace ManualDi.Sync.Tests;

[TestFixture]
public class TestsSourceGenerator
{
    [Test]
    public Task Generated()
    {
        var code = File.ReadAllText("TestSourceGenerator.Source.cs");
        var generated = Generate(code);
        Assert.That(generated.diagnostics, Is.Empty);
        return Verifier.Verify(generated.code);
    }

    private static (IEnumerable<string> code, ImmutableArray<Diagnostic> diagnostics) Generate(string code)
    {
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location) && !a.Location.Contains("ManualDi.Sync.Tests.dll"))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();
        
        references.Add(MetadataReference.CreateFromFile(typeof(IDiContainer).Assembly.Location));
        
        var compilation = CSharpCompilation.Create("AssemblyName",
            new[] { CSharpSyntaxTree.ParseText(SourceText.From(code, Encoding.UTF8)) },
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