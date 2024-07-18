using ManualDi.Main.Generators;

namespace ManualDi.Main.Tests;

using System.Linq;
using NUnit.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;

public class FromDefaultSourceGeneratorTests
{
    [Test]
    public void TestFromDefaultSourceGenerator()
    {
        var code = @"
namespace SomeNamespace; //We are testing that it properly picks up namespaces

public class MyClass
{
    public MyClass(OtherClass otherClass) { }
}

public class OtherClass 
{
}
";

        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(code, Encoding.UTF8));
        var compilation = CSharpCompilation.Create("Test Assembly-34#",
            new[] { syntaxTree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new FromDefaultSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        var generatedTrees = outputCompilation.SyntaxTrees.ToList();
        Assert.AreEqual(2, generatedTrees.Count); // Original + generated

        var generatedCode = generatedTrees.Skip(1).First().ToString();
        var expectedCode = @"
using ManualDi.Main;

namespace ManualDi.Main
{
    public static class ManualDiFromDefaultExtensions_TestAssembly34
    {

        public static TypeBinding<T, SomeNamespace.MyClass> FromDefault<T>(this TypeBinding<T, SomeNamespace.MyClass> typeBinding)
            where SomeNamespace.MyClass : T
        {
            typeBinding.FromMethod(static c => new SomeNamespace.MyClass(
                c.Resolve<SomeNamespace.OtherClass>()));
            return typeBinding;
        }

        public static TypeBinding<T, SomeNamespace.OtherClass> FromDefault<T>(this TypeBinding<T, SomeNamespace.OtherClass> typeBinding)
            where SomeNamespace.OtherClass : T
        {
            typeBinding.FromMethod(static c => new SomeNamespace.OtherClass(
                ));
            return typeBinding;
        }

    }
}
";
        
        Assert.AreEqual(expectedCode.Trim(), generatedCode.Trim());
    }
}