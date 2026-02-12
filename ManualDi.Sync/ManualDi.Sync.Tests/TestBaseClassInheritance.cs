
using NUnit.Framework;
using ManualDi.Sync.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace ManualDi.Sync.Tests
{
    public class TestBaseClassInheritance
    {
        [Test]
        public void TestBaseClassWithoutManualDiAttribute()
        {
            var code = @"
using ManualDi.Sync;

namespace ManualDi.Sync.Tests
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
            Assert.That(generated, Does.Not.Contain("ManualDi_ManualDi_Sync_Tests_AbstractViewLoader"));
        }
    }
}
