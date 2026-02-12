using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Threading.Tasks;
using ManualDi.Sync.Generators;
using VerifyNUnit;

namespace ManualDi.Sync.Tests;

[TestFixture]
public class TestsSourceGenerator
{
    [Test]
    public Task Generated()
    {
        var code = File.ReadAllText("TestSourceGenerator.Source.cs");
        var generated = GeneratorTestHelper.Generate(code);
        Assert.That(generated.diagnostics, Is.Empty);
        return Verifier.Verify(generated.code);
    }
}