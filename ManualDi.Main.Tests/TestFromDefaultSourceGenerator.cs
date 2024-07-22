using ManualDi.Main.Generators;

namespace ManualDi.Main.Tests;

using System.Linq;
using NUnit.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;

public class ManualDiSourceGeneratorTests
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

        var output = Generate(code);

        var generatedTrees = output.SyntaxTrees.ToList();
        Assert.AreEqual(2, generatedTrees.Count); // Original + generated

        var generatedCode = generatedTrees.ElementAt(1).ToString();
        var expectedCode = @"
using ManualDi.Main;

namespace ManualDi.Main
{
    public static partial class ManualDiGeneratedExtensions
    {

        public static TypeBinding<T, global::SomeNamespace.MyClass> FromConstructor<T>(this TypeBinding<T, global::SomeNamespace.MyClass> typeBinding)
        {
            typeBinding.FromMethod(static c => new global::SomeNamespace.MyClass(c.Resolve<global::SomeNamespace.OtherClass>()));
            return typeBinding;
        }

        public static TypeBinding<T, global::SomeNamespace.MyClass> Default<T>(this TypeBinding<T, global::SomeNamespace.MyClass> typeBinding)
        {
            typeBinding.FromConstructor();

            return typeBinding;
        }

        public static TypeBinding<T, global::SomeNamespace.OtherClass> FromConstructor<T>(this TypeBinding<T, global::SomeNamespace.OtherClass> typeBinding)
        {
            typeBinding.FromMethod(static c => new global::SomeNamespace.OtherClass());
            return typeBinding;
        }

        public static TypeBinding<T, global::SomeNamespace.OtherClass> Default<T>(this TypeBinding<T, global::SomeNamespace.OtherClass> typeBinding)
        {
            typeBinding.FromConstructor();

            return typeBinding;
        }

    }
}
";
        
        Assert.AreEqual(expectedCode.Trim(), generatedCode.Trim());
    }
    
    [Test]
    public void TestInitializeGenerator()
    {
        var code = @"
public class MyClass
{
    public void Initialize(OtherClass otherClass, OtherClass otherClass) { }
}

public class OtherClass 
{
    public void Initialize() { }
}
";

        var outputCompilation = Generate(code);

        var generatedTrees = outputCompilation.SyntaxTrees.ToList();
        Assert.AreEqual(2, generatedTrees.Count); // Original + generated

        var generatedCode = generatedTrees.Skip(1).First().ToString();
        
        var expectedCode = @"
using ManualDi.Main;

namespace ManualDi.Main
{
    public static partial class ManualDiGeneratedExtensions
    {

        public static TypeBinding<T, global::MyClass> FromConstructor<T>(this TypeBinding<T, global::MyClass> typeBinding)
        {
            typeBinding.FromMethod(static c => new global::MyClass());
            return typeBinding;
        }

        public static TypeBinding<T, global::MyClass> Initialize<T>(this TypeBinding<T, global::MyClass> typeBinding)
        {
            typeBinding.Initialize(static (o, c) => o.Initialize(c.Resolve<global::OtherClass>(),
                c.Resolve<global::OtherClass>()));
            return typeBinding;
        }

        public static TypeBinding<T, global::MyClass> Default<T>(this TypeBinding<T, global::MyClass> typeBinding)
        {
            typeBinding.FromConstructor();
            typeBinding.Initialize();

            return typeBinding;
        }

        public static TypeBinding<T, global::OtherClass> FromConstructor<T>(this TypeBinding<T, global::OtherClass> typeBinding)
        {
            typeBinding.FromMethod(static c => new global::OtherClass());
            return typeBinding;
        }

        public static TypeBinding<T, global::OtherClass> Initialize<T>(this TypeBinding<T, global::OtherClass> typeBinding)
        {
            typeBinding.Initialize(static (o, c) => o.Initialize());
            return typeBinding;
        }

        public static TypeBinding<T, global::OtherClass> Default<T>(this TypeBinding<T, global::OtherClass> typeBinding)
        {
            typeBinding.FromConstructor();
            typeBinding.Initialize();

            return typeBinding;
        }

    }
}
";
        
        Assert.AreEqual(expectedCode.Trim(), generatedCode.Trim());
    }

    private static Compilation Generate(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(code, Encoding.UTF8));
        var compilation = CSharpCompilation.Create("AssemblyName",
            new[] { syntaxTree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ManualDiSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        return outputCompilation;
    }
}