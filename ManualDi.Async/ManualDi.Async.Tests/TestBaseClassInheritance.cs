
using NUnit.Framework;
using System.Linq;

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
    }
}
