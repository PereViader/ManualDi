
using NUnit.Framework;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using ManualDi.Async.Generators;

namespace ManualDi.Async.Tests
{
    public class TestBaseClassInheritance
    {
        [Test]
        public void TestBaseClassWithoutManualDiAttribute()
        {
            var code = @"
using ManualDi.Async;
using System.Threading.Tasks;

namespace ManualDi.Async.Tests
{
    public class AbstractViewLoader<T> 
    {
    }

    [ManualDi]
    public class ConcreteViewLoader : AbstractViewLoader<int>
    {
    }
}
";
            var (generatedCode, diagnostics) = GeneratorTestHelper.Generate(code);

            // We expect the generated code for ConcreteViewLoader NOT to call ManualDi_AbstractViewLoader_..._Extensions.DefaultImpl
            // because AbstractViewLoader does not have [ManualDi].

            var generated = generatedCode.FirstOrDefault(x => x.Contains("ConcreteViewLoader"));
            Assert.That(generated, Is.Not.Null, "Should generate code for ConcreteViewLoader");

            // Check that it does NOT contain a call to the base extension
            Assert.That(generated, Does.Not.Contain("ManualDi_ManualDi_Async_Tests_AbstractViewLoader"));
        }

        [Test]
        public void TestBaseClassWithGenericRequirements()
        {
            var code = @"
using ManualDi.Async;
using System.Threading.Tasks;

namespace ManualDi.Async.Tests
{
    public interface ISomething {}
    public class Other : ISomething {}

    [ManualDi]
    public class Base<T> where T : ISomething
    {
    }

    [ManualDi]
    public class Something : Base<Other>
    {
    }
}
";
            var (generatedCode, diagnostics) = GeneratorTestHelper.Generate(code);

            var errors = diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).ToList();
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void TestBaseClassWithMultipleGenericRequirements()
        {
            var code = @"
using ManualDi.Async;
using System.Threading.Tasks;

namespace ManualDi.Async.Tests
{
    public interface ISomething {}
    public class Other : ISomething {}

    [ManualDi]
    public class Base<TKey, TValue> where TValue : ISomething
    {
    }

    [ManualDi]
    public class Something : Base<string, Other>
    {
    }
}
";
            var (generatedCode, diagnostics) = GeneratorTestHelper.Generate(code);

            var errors = diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).ToList();
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void TestBaseClassWithGenericSubclass()
        {
            var code = @"
using ManualDi.Async;
using System.Threading.Tasks;

namespace ManualDi.Async.Tests
{
    public interface ISomething {}

    [ManualDi]
    public class Base<T> where T : ISomething
    {
    }

    [ManualDi]
    public class Something<T> : Base<T> where T : ISomething
    {
    }
}
";
            var (generatedCode, diagnostics) = GeneratorTestHelper.Generate(code);

            var errors = diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).ToList();
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void TestBaseClassWithGenericInject()
        {
            var code = @"
using ManualDi.Async;
using System.Threading.Tasks;

namespace ManualDi.Async.Tests
{
    public interface ISomething {}
    public class Other : ISomething {}

    [ManualDi]
    public class Base<T> where T : ISomething
    {
        public void Inject(T dependency) {}
    }

    [ManualDi]
    public class Something : Base<Other>
    {
    }
}
";
            var (generatedCode, diagnostics) = GeneratorTestHelper.Generate(code);

            var errors = diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).ToList();
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void TestBaseClassInDifferentAssembly()
        {
            var baseCode = @"
using ManualDi.Async;
using System.Threading.Tasks;

namespace AssemblyA
{
    public interface ISomething {}
    public class Other : ISomething {}

    [ManualDi]
    public class Base<T> where T : ISomething
    {
    }
}
";
            var references = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location) && !a.Location.Contains("ManualDi.Async.Tests.dll"))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();
            references.Add(MetadataReference.CreateFromFile(typeof(IDiContainer).Assembly.Location));

            var compA = CSharpCompilation.Create("AssemblyA",
                [CSharpSyntaxTree.ParseText(baseCode)],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var driverA = CSharpGeneratorDriver.Create(new ManualDiSourceGenerator());
            driverA.RunGeneratorsAndUpdateCompilation(compA, out var outputCompA, out _);

            using var ms = new System.IO.MemoryStream();
            var emitResult = outputCompA.Emit(ms);
            Assert.That(emitResult.Success, Is.True);
            ms.Position = 0;
            var refA = MetadataReference.CreateFromStream(ms);

            var subclassCode = @"
using AssemblyA;
using ManualDi.Async;
using System.Threading.Tasks;

namespace AssemblyB
{
    [ManualDi]
    public class Something : Base<Other>
    {
    }
}
";
            var referencesB = new List<MetadataReference>(references) { refA };
            var compB = CSharpCompilation.Create("AssemblyB",
                [CSharpSyntaxTree.ParseText(subclassCode)],
                referencesB,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var driver = CSharpGeneratorDriver.Create(new ManualDiSourceGenerator());
            driver.RunGeneratorsAndUpdateCompilation(compB, out var outputComp, out _);

            var errors = outputComp.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).ToList();
            foreach (var error in errors)
            {
                Console.WriteLine("ERROR: " + error);
            }
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void TestBaseClassWithNullableClassConstraint()
        {
            var code = @"
using ManualDi.Async;
using System.Threading.Tasks;

namespace ManualDi.Async.Tests
{
    [ManualDi]
    public class Base<T> where T : class?
    {
    }

    [ManualDi]
    public class Something : Base<string>
    {
    }
}
";
            var (generatedCode, diagnostics) = GeneratorTestHelper.Generate(code);

            var errors = diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error || x.Severity == DiagnosticSeverity.Warning).ToList();
            foreach (var error in errors)
            {
                Console.WriteLine("DIAGNOSTIC: " + error);
            }
            Assert.That(errors, Is.Empty);
        }
    }
}
