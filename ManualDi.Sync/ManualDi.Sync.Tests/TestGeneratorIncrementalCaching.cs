using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManualDi.Sync.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace ManualDi.Sync.Tests;

[TestFixture]
public class TestGeneratorIncrementalCaching
{
    private static readonly List<MetadataReference> References = CreateReferences();

    private static List<MetadataReference> CreateReferences()
    {
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location) && !a.Location.Contains("ManualDi.Sync.Tests.dll"))
            .Select(a => (MetadataReference)MetadataReference.CreateFromFile(a.Location))
            .ToList();

        references.Add(MetadataReference.CreateFromFile(typeof(IDiContainer).Assembly.Location));
        return references;
    }

    private static (string generated1, string generated2, GeneratorRunResult runResult) RunIncremental(
        IIncrementalGenerator generator,
        string initialCode,
        string updatedCode)
    {
        var tree1 = CSharpSyntaxTree.ParseText(SourceText.From(initialCode, Encoding.UTF8));
        var compilation1 = CSharpCompilation.Create(
            "AssemblyName",
            [tree1],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [generator.AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(default, trackIncrementalGeneratorSteps: true));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation1, out var outputCompilation1, out var genDiag1);
        Assert.That(genDiag1, Is.Empty);
        Assert.That(outputCompilation1.GetDiagnostics(), Is.Empty);

        var tree2 = CSharpSyntaxTree.ParseText(SourceText.From(updatedCode, Encoding.UTF8));
        var compilation2 = compilation1.ReplaceSyntaxTree(tree1, tree2);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation2, out var outputCompilation2, out var genDiag2);
        Assert.That(genDiag2, Is.Empty);
        Assert.That(outputCompilation2.GetDiagnostics(), Is.Empty);

        var generated1 = string.Join("\n", outputCompilation1.SyntaxTrees.Skip(1).Select(t => t.ToString()));
        var generated2 = string.Join("\n", outputCompilation2.SyntaxTrees.Skip(1).Select(t => t.ToString()));
        var runResult = driver.GetRunResult().Results[0];

        return (generated1, generated2, runResult);
    }

    private static List<IncrementalStepRunReason> GetOutputReasons(GeneratorRunResult runResult)
    {
        return runResult.TrackedOutputSteps.Values
            .SelectMany(steps => steps)
            .SelectMany(step => step.Outputs)
            .Select(output => output.Reason)
            .ToList();
    }

    [Test]
    public void ManualDiSourceGenerator_ConstructorVisibility_PublicToPrivate_Regenerates()
    {
        const string publicCtor = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDi]
                public class Service
                {
                    public Service() { }
                }
            }
            """;

        const string privateCtor = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDi]
                public class Service
                {
                    private Service() { }
                }
            }
            """;

        var (gen1, gen2, runResult) = RunIncremental(new ManualDiSourceGenerator(), publicCtor, privateCtor);

        Assert.That(gen1, Does.Contain("FromConstructor"));
        Assert.That(gen2, Does.Not.Contain("FromConstructor"));
        Assert.That(GetOutputReasons(runResult), Has.Some.Matches<IncrementalStepRunReason>(r => r is IncrementalStepRunReason.Modified or IncrementalStepRunReason.New));
    }

    [Test]
    public void ManualDiSourceGenerator_ConstructorVisibility_PrivateToPublic_Regenerates()
    {
        const string privateCtor = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDi]
                public class Service
                {
                    private Service() { }
                }
            }
            """;

        const string publicCtor = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDi]
                public class Service
                {
                    public Service() { }
                }
            }
            """;

        var (gen1, gen2, runResult) = RunIncremental(new ManualDiSourceGenerator(), privateCtor, publicCtor);

        Assert.That(gen1, Does.Not.Contain("FromConstructor"));
        Assert.That(gen2, Does.Contain("FromConstructor"));
        Assert.That(GetOutputReasons(runResult), Has.Some.Matches<IncrementalStepRunReason>(r => r is IncrementalStepRunReason.Modified or IncrementalStepRunReason.New));
    }

    [Test]
    public void ManualDiSourceGenerator_ParameterlessInject_AbsentToPresent_Regenerates()
    {
        const string withoutInject = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDi]
                public class Service
                {
                }
            }
            """;

        const string withInject = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDi]
                public class Service
                {
                    public void Inject() { }
                }
            }
            """;

        var (gen1, gen2, runResult) = RunIncremental(new ManualDiSourceGenerator(), withoutInject, withInject);

        Assert.That(gen1, Does.Not.Contain(".Inject("));
        Assert.That(gen2, Does.Contain(".Inject("));
        Assert.That(gen2, Does.Contain("to.Inject("));
        Assert.That(GetOutputReasons(runResult), Has.Some.Matches<IncrementalStepRunReason>(r => r is IncrementalStepRunReason.Modified or IncrementalStepRunReason.New));
    }

    [Test]
    public void ManualDiSourceGenerator_ParameterlessInject_PresentToAbsent_Regenerates()
    {
        const string withInject = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDi]
                public class Service
                {
                    public void Inject() { }
                }
            }
            """;

        const string withoutInject = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDi]
                public class Service
                {
                }
            }
            """;

        var (gen1, gen2, runResult) = RunIncremental(new ManualDiSourceGenerator(), withInject, withoutInject);

        Assert.That(gen1, Does.Contain(".Inject("));
        Assert.That(gen2, Does.Not.Contain(".Inject("));
        Assert.That(GetOutputReasons(runResult), Has.Some.Matches<IncrementalStepRunReason>(r => r is IncrementalStepRunReason.Modified or IncrementalStepRunReason.New));
    }

    [Test]
    public void ManualDiSourceGenerator_ParameterlessInject_VisibilityToggle_Regenerates()
    {
        const string publicInject = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDi]
                public class Service
                {
                    public void Inject() { }
                }
            }
            """;

        const string privateInject = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDi]
                public class Service
                {
                    private void Inject() { }
                }
            }
            """;

        var (gen1, gen2, runResult1) = RunIncremental(new ManualDiSourceGenerator(), publicInject, privateInject);
        Assert.That(gen1, Does.Contain(".Inject("));
        Assert.That(gen2, Does.Not.Contain(".Inject("));
        Assert.That(GetOutputReasons(runResult1), Has.Some.Matches<IncrementalStepRunReason>(r => r is IncrementalStepRunReason.Modified or IncrementalStepRunReason.New));

        var (gen3, gen4, runResult2) = RunIncremental(new ManualDiSourceGenerator(), privateInject, publicInject);
        Assert.That(gen3, Does.Not.Contain(".Inject("));
        Assert.That(gen4, Does.Contain(".Inject("));
        Assert.That(GetOutputReasons(runResult2), Has.Some.Matches<IncrementalStepRunReason>(r => r is IncrementalStepRunReason.Modified or IncrementalStepRunReason.New));
    }

    [Test]
    public void ManualDiSourceGenerator_UnrelatedEdit_RemainsCached()
    {
        const string initial = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDi]
                public class Service
                {
                    public void Inject() { }
                    public int Compute() => 1;
                }
            }
            """;

        const string updated = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDi]
                public class Service
                {
                    public void Inject() { }
                    public int Compute() => 2;
                }
            }
            """;

        var (_, _, runResult) = RunIncremental(new ManualDiSourceGenerator(), initial, updated);

        var reasons = GetOutputReasons(runResult);
        Assert.That(reasons, Is.Not.Empty);
        Assert.That(reasons, Is.All.EqualTo(IncrementalStepRunReason.Cached));
    }

    [Test]
    public void ManualDiInjectableSourceGenerator_ParameterlessInject_AbsentToPresent_And_PresentToAbsent_Regenerates()
    {
        const string withoutInject = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDiInjectable]
                public class Service
                {
                }
            }
            """;

        const string withInject = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDiInjectable]
                public class Service
                {
                    public void Inject() { }
                }
            }
            """;

        var (gen1, gen2, runResult1) = RunIncremental(new ManualDiInjectableSourceGenerator(), withoutInject, withInject);
        Assert.That(gen1, Does.Not.Contain("target.Inject("));
        Assert.That(gen2, Does.Contain("target.Inject("));
        Assert.That(GetOutputReasons(runResult1), Has.Some.Matches<IncrementalStepRunReason>(r => r is IncrementalStepRunReason.Modified or IncrementalStepRunReason.New));

        var (gen3, gen4, runResult2) = RunIncremental(new ManualDiInjectableSourceGenerator(), withInject, withoutInject);
        Assert.That(gen3, Does.Contain("target.Inject("));
        Assert.That(gen4, Does.Not.Contain("target.Inject("));
        Assert.That(GetOutputReasons(runResult2), Has.Some.Matches<IncrementalStepRunReason>(r => r is IncrementalStepRunReason.Modified or IncrementalStepRunReason.New));
    }

    [Test]
    public void ManualDiInjectableSourceGenerator_ParameterlessInject_VisibilityToggle_Regenerates()
    {
        const string publicInject = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDiInjectable]
                public class Service
                {
                    public void Inject() { }
                }
            }
            """;

        const string privateInject = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDiInjectable]
                public class Service
                {
                    private void Inject() { }
                }
            }
            """;

        var (gen1, gen2, runResult1) = RunIncremental(new ManualDiInjectableSourceGenerator(), publicInject, privateInject);
        Assert.That(gen1, Does.Contain("target.Inject("));
        Assert.That(gen2, Does.Not.Contain("target.Inject("));
        Assert.That(GetOutputReasons(runResult1), Has.Some.Matches<IncrementalStepRunReason>(r => r is IncrementalStepRunReason.Modified or IncrementalStepRunReason.New));

        var (gen3, gen4, runResult2) = RunIncremental(new ManualDiInjectableSourceGenerator(), privateInject, publicInject);
        Assert.That(gen3, Does.Not.Contain("target.Inject("));
        Assert.That(gen4, Does.Contain("target.Inject("));
        Assert.That(GetOutputReasons(runResult2), Has.Some.Matches<IncrementalStepRunReason>(r => r is IncrementalStepRunReason.Modified or IncrementalStepRunReason.New));
    }

    [Test]
    public void ManualDiInjectableSourceGenerator_UnrelatedEdit_RemainsCached()
    {
        const string initial = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDiInjectable]
                public class Service
                {
                    public void Inject() { }
                    public int Compute() => 1;
                }
            }
            """;

        const string updated = """
            using ManualDi.Sync;
            namespace TestNs
            {
                [ManualDiInjectable]
                public class Service
                {
                    public void Inject() { }
                    public int Compute() => 2;
                }
            }
            """;

        var (_, _, runResult) = RunIncremental(new ManualDiInjectableSourceGenerator(), initial, updated);

        var reasons = GetOutputReasons(runResult);
        Assert.That(reasons, Is.Not.Empty);
        Assert.That(reasons, Is.All.EqualTo(IncrementalStepRunReason.Cached));
    }
}
