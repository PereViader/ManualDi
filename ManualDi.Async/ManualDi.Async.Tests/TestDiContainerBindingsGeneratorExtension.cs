using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Async.Tests
{
    public interface ICustomExtension
    {
        List<string> CallLog { get; }
    }

    public static class CustomExtensionManualDiExtensions
    {
        [ManualDiGeneratorExtension]
        public static Binding<TConcrete> LinkCustomExtension<TConcrete>(this Binding<TConcrete> binding)
            where TConcrete : ICustomExtension
        {
            return binding
                .Initialize(o => ((ICustomExtension)o).CallLog.Add("Initialize"))
                .Dispose(o => ((ICustomExtension)o).CallLog.Add("Dispose"));
        }
    }

    [ManualDi]
    public class CustomExtensionTarget : ICustomExtension
    {
        public List<string> CallLog { get; } = new();
    }

    [ManualDi]
    public class BaseCustomExtensionTarget : ICustomExtension
    {
        public List<string> CallLog { get; } = new();
    }

    [ManualDi]
    public class ChildCustomExtensionTarget : BaseCustomExtensionTarget
    {
    }

    public class TestDiContainerBindingsGeneratorExtension
    {
        [Test]
        public async Task TestGeneratorExtension_RunsHook()
        {
            var callLog = new List<string>();
            await using (var diContainer = await new DiContainerBindings().Install(b =>
            {
                b.Bind<CustomExtensionTarget>().Default().FromConstructor();
            }).Build(CancellationToken.None))
            {
                var instance = diContainer.Resolve<CustomExtensionTarget>();
                callLog = instance.CallLog;
                Assert.That(callLog, Is.EquivalentTo(new[] { "Initialize" }));
            }
            Assert.That(callLog, Is.EquivalentTo(new[] { "Initialize", "Dispose" }));
        }

        [Test]
        public async Task TestGeneratorExtension_DeduplicatesHookOnInheritance()
        {
            var callLog = new List<string>();
            await using (var diContainer = await new DiContainerBindings().Install(b =>
            {
                b.Bind<ChildCustomExtensionTarget>().Default().FromConstructor();
            }).Build(CancellationToken.None))
            {
                var instance = diContainer.Resolve<ChildCustomExtensionTarget>();
                callLog = instance.CallLog;
                // Because BaseCustomExtensionTarget defines DefaultImpl and ChildCustomExtensionTarget inherits/calls it,
                // and ChildCustomExtensionTarget deduplicates, it should only register and call the hook once!
                Assert.That(callLog, Is.EquivalentTo(new[] { "Initialize" }));
            }
            Assert.That(callLog, Is.EquivalentTo(new[] { "Initialize", "Dispose" }));
        }
    }
}
