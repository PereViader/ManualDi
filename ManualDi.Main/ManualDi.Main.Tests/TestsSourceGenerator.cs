using System.Collections.Generic;
using System.IO;
using ManualDi.Main.Generators;
using System.Linq;
using NUnit.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Threading.Tasks;
using VerifyNUnit;

namespace ManualDi.Main.Tests;

[TestFixture]
public class TestsSourceGenerator
{
    [Test]
    public Task Generated()
    {
        var code = File.ReadAllText("TestsSourceGenerator.Source.txt");
        var generatedCode = Generate(code);
        return Verifier.Verify(generatedCode);
    }

    private static IEnumerable<string> Generate(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(code, Encoding.UTF8));
        var compilation = CSharpCompilation.Create("AssemblyName",
            new[] { syntaxTree },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IDiContainer).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new Generators.TestsSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        
        var generatedTrees = outputCompilation.SyntaxTrees.ToList();

        var generatedCode = generatedTrees.Skip(1).Select(x => x.ToString());
        return generatedCode;
    }
}