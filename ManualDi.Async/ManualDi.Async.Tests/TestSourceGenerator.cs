using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using ManualDi.Async.Generators;
using System.Linq;
using NUnit.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Threading.Tasks;
using VerifyNUnit;

namespace ManualDi.Async.Tests;

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