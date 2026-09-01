using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ManualDi.Async.Tests
{
    public class TestManualDiInjector
    {
        [ManualDiInjectable]
        public class SimpleTarget
        {
            public string? Name { get; private set; }
            public int Value { get; private set; }
            public IDiContainer? Container { get; private set; }
            public CancellationToken CancellationToken { get; private set; }

            public void Inject(string name, int value, IDiContainer container, CancellationToken cancellationToken)
            {
                Name = name;
                Value = value;
                Container = container;
                CancellationToken = cancellationToken;
            }
        }

        [Test]
        public async Task TestSimpleInjection()
        {
            var diContainer = await new DiContainerBindings().Install(b =>
            {
                b.Bind<string>().FromInstance("hello");
                b.Bind<int>().FromInstance(42);
            }).Build(CancellationToken.None);
            await using var _ = diContainer;

            var target = new SimpleTarget();
            Assert.That(ManualDiInjector.CanInject(target), Is.True);
            Assert.That(ManualDiInjector.CanInject(typeof(SimpleTarget)), Is.True);

            ManualDiInjector.Inject(target, diContainer);

            Assert.That(target.Name, Is.EqualTo("hello"));
            Assert.That(target.Value, Is.EqualTo(42));
            Assert.That(target.Container, Is.SameAs(diContainer));
            Assert.That(target.CancellationToken, Is.EqualTo(diContainer.CancellationToken));
        }

        // --- Inheritance with in-between type without attribute ---

        [ManualDiInjectable]
        public class BaseA
        {
            public List<string> CallOrder { get; } = new();

            public void Inject(string message)
            {
                CallOrder.Add($"BaseA:{message}");
            }
        }

        public class MiddleB : BaseA
        {
            public void Inject(int ignore)
            {
                CallOrder.Add($"MiddleB:{ignore}");
            }
        }

        [ManualDiInjectable]
        public class ChildC : MiddleB
        {
            public new void Inject(int count)
            {
                CallOrder.Add($"ChildC:{count}");
            }
        }

        [Test]
        public async Task TestInheritanceWithInBetweenUnmarkedClass()
        {
            var diContainer = await new DiContainerBindings().Install(b =>
            {
                b.Bind<string>().FromInstance("message_from_di");
                b.Bind<int>().FromInstance(123);
            }).Build(CancellationToken.None);
            await using var _ = diContainer;

            // 1. ChildC is decorated and should inject BaseA then ChildC, skipping MiddleB
            var child = new ChildC();
            Assert.That(ManualDiInjector.CanInject(child), Is.True);
            ManualDiInjector.Inject(child, diContainer);

            Assert.That(child.CallOrder, Is.EqualTo(new[]
            {
                "BaseA:message_from_di",
                "ChildC:123"
            }));

            // 2. MiddleB is NOT decorated and should not be registered
            var middle = new MiddleB();
            Assert.That(ManualDiInjector.CanInject(middle), Is.False);
            Assert.Throws<InvalidOperationException>(() => ManualDiInjector.Inject(middle, diContainer));

            // 3. BaseA is decorated and should inject BaseA only
            var baseA = new BaseA();
            Assert.That(ManualDiInjector.CanInject(baseA), Is.True);
            ManualDiInjector.Inject(baseA, diContainer);
            Assert.That(baseA.CallOrder, Is.EqualTo(new[]
            {
                "BaseA:message_from_di"
            }));
        }

        // --- Generic base class inheritance ---

        [ManualDiInjectable]
        public class GenericBase<T>
        {
            public List<string> InjectedValues { get; } = new();

            public void Inject(T value)
            {
                InjectedValues.Add($"GenericBase:{value}");
            }
        }

        public class InBetweenGeneric<T> : GenericBase<T>
        {
        }

        [ManualDiInjectable]
        public class ConcreteChild : InBetweenGeneric<string>
        {
            public void Inject(int number)
            {
                InjectedValues.Add($"ConcreteChild:{number}");
            }
        }

        [Test]
        public async Task TestGenericBaseInheritance()
        {
            var diContainer = await new DiContainerBindings().Install(b =>
            {
                b.Bind<string>().FromInstance("generic_arg");
                b.Bind<int>().FromInstance(555);
            }).Build(CancellationToken.None);
            await using var _ = diContainer;

            var concrete = new ConcreteChild();
            ManualDiInjector.Inject(concrete, diContainer);

            Assert.That(concrete.InjectedValues, Is.EqualTo(new[]
            {
                "GenericBase:generic_arg",
                "ConcreteChild:555"
            }));
        }

        // --- Multi-level skipping ---

        [ManualDiInjectable]
        public class Level1
        {
            public List<string> Calls { get; } = new();

            public void Inject(string s)
            {
                Calls.Add($"L1:{s}");
            }
        }

        public class Level2 : Level1
        {
        }

        public class Level3 : Level2
        {
        }

        [ManualDiInjectable]
        public class Level4 : Level3
        {
            public void Inject(int i)
            {
                Calls.Add($"L4:{i}");
            }
        }

        [Test]
        public async Task TestMultiLevelSkipping()
        {
            var diContainer = await new DiContainerBindings().Install(b =>
            {
                b.Bind<string>().FromInstance("deep");
                b.Bind<int>().FromInstance(4);
            }).Build(CancellationToken.None);
            await using var _ = diContainer;

            var l4 = new Level4();
            ManualDiInjector.Inject(l4, diContainer);

            Assert.That(l4.Calls, Is.EqualTo(new[]
            {
                "L1:deep",
                "L4:4"
            }));
        }

        // --- Abstract base class ---

        [ManualDiInjectable]
        public abstract class AbstractBaseClass
        {
            public string? BaseString { get; private set; }

            public void Inject(string s)
            {
                BaseString = s;
            }
        }

        [ManualDiInjectable]
        public class DerivedFromAbstract : AbstractBaseClass
        {
            public int DerivedInt { get; private set; }

            public void Inject(int i)
            {
                DerivedInt = i;
            }
        }

        [Test]
        public async Task TestAbstractBaseClass()
        {
            var diContainer = await new DiContainerBindings().Install(b =>
            {
                b.Bind<string>().FromInstance("abstract_base");
                b.Bind<int>().FromInstance(77);
            }).Build(CancellationToken.None);
            await using var _ = diContainer;

            var derived = new DerivedFromAbstract();
            ManualDiInjector.Inject(derived, diContainer);

            Assert.That(derived.BaseString, Is.EqualTo("abstract_base"));
            Assert.That(derived.DerivedInt, Is.EqualTo(77));
            Assert.That(ManualDiInjector.CanInject(typeof(AbstractBaseClass)), Is.False);
        }

        // --- Target without direct Inject method ---

        [ManualDiInjectable]
        public class BaseWithInject
        {
            public string? InjectedMessage { get; private set; }

            public void Inject(string message)
            {
                InjectedMessage = message;
            }
        }

        [ManualDiInjectable]
        public class DerivedWithoutInject : BaseWithInject
        {
        }

        [Test]
        public async Task TestDerivedWithoutDirectInjectMethod()
        {
            var diContainer = await new DiContainerBindings().Install(b =>
            {
                b.Bind<string>().FromInstance("only_base");
            }).Build(CancellationToken.None);
            await using var _ = diContainer;

            var derived = new DerivedWithoutInject();
            ManualDiInjector.Inject(derived, diContainer);

            Assert.That(derived.InjectedMessage, Is.EqualTo("only_base"));
        }

        // --- Complex parameter resolutions: Keyed, Nullable, Collections, Out ---

        public struct KeyA { }
        public struct KeyB { }

        public class UnboundService { }

        [ManualDiInjectable]
        public class ComplexParametersTarget
        {
            public string? KeyedA { get; private set; }
            public string? KeyedB { get; private set; }
            public int? NullableInt { get; private set; }
            public double? NullableDouble { get; private set; }
            public string? NullableString { get; private set; }
            public UnboundService? NullableUnbound { get; private set; }
            public List<int>? IntList { get; private set; }
            public string[]? StringArray { get; private set; }

            public void Inject(
                [Keyed(typeof(KeyA))] string keyedA,
                [Keyed(typeof(KeyB))] string? keyedB,
                int? nullableInt,
                double? nullableDouble,
                string? nullableString,
                UnboundService? nullableUnbound,
                List<int> intList,
                string[] stringArray,
                out string outParam)
            {
                KeyedA = keyedA;
                KeyedB = keyedB;
                NullableInt = nullableInt;
                NullableDouble = nullableDouble;
                NullableString = nullableString;
                NullableUnbound = nullableUnbound;
                IntList = intList;
                StringArray = stringArray;
                outParam = "out_value";
            }
        }

        [Test]
        public async Task TestComplexParameterResolutions()
        {
            var diContainer = await new DiContainerBindings().Install(b =>
            {
                b.BindKeyed<string, KeyA>().FromInstance("keyed_a_value");
                b.Bind<int>().FromInstance(1);
                b.Bind<int>().FromInstance(2);
                b.Bind<string>().FromInstance("str1");
                b.Bind<string>().FromInstance("str2");
            }).Build(CancellationToken.None);
            await using var _ = diContainer;

            var target = new ComplexParametersTarget();
            ManualDiInjector.Inject(target, diContainer);

            Assert.That(target.KeyedA, Is.EqualTo("keyed_a_value"));
            Assert.That(target.KeyedB, Is.Null);
            Assert.That(target.NullableInt, Is.EqualTo(1));
            Assert.That(target.NullableDouble, Is.Null);
            Assert.That(target.NullableString, Is.EqualTo("keyed_a_value"));
            Assert.That(target.NullableUnbound, Is.Null);
            Assert.That(target.IntList, Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(target.StringArray, Is.EquivalentTo(new[] { "keyed_a_value", "str1", "str2" }));
        }

        // --- Exception handling and CanInject ---

        public class UnregisteredClass
        {
        }

        [Test]
        public async Task TestUnregisteredType()
        {
            var diContainer = await new DiContainerBindings().Build(CancellationToken.None);
            await using var _ = diContainer;
            var unregistered = new UnregisteredClass();

            Assert.That(ManualDiInjector.CanInject(unregistered), Is.False);
            Assert.That(ManualDiInjector.CanInject(typeof(UnregisteredClass)), Is.False);
            Assert.Throws<InvalidOperationException>(() => ManualDiInjector.Inject(unregistered, diContainer));
        }

        // --- Generator diagnostic tests ---

        [Test]
        public void TestGeneratorCodeGeneration()
        {
            var code = @"
using ManualDi.Async;

namespace TestNamespace
{
    [ManualDiInjectable]
    public class Base
    {
        public void Inject(string str) {}
    }

    public class Middle : Base
    {
    }

    [ManualDiInjectable]
    public class Child : Middle
    {
        public void Inject(int num) {}
    }
}
";
            var (generatedCode, diagnostics) = GeneratorTestHelper.GenerateInjectable(code);
            Assert.That(diagnostics, Is.Empty);

            var codeList = new List<string>(generatedCode);
            Assert.That(codeList.Count, Is.GreaterThanOrEqualTo(3)); // Base injector, Child injector, Registration
            Assert.That(codeList.Exists(x => x.Contains("ManualDi_TestNamespace_Base_Injector")), Is.True);
            Assert.That(codeList.Exists(x => x.Contains("ManualDi_TestNamespace_Child_Injector")), Is.True);
            Assert.That(codeList.Exists(x => x.Contains("ManualDi_Injection_Registration")), Is.True);
            Assert.That(codeList.Exists(x => x.Contains("ManualDi_TestNamespace_Base_Injector.Inject(target, container)")), Is.True);
        }

        [Test]
        public void TestGeneratorUnityAwareCodeGeneration()
        {
            var code = @"
using ManualDi.Async;

namespace UnityEngine.Scripting
{
    public class PreserveAttribute : System.Attribute {}
}

namespace UnityEngine
{
    public enum RuntimeInitializeLoadType { SubsystemRegistration }
    public class RuntimeInitializeOnLoadMethodAttribute : System.Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType type) {}
    }
}

namespace TestUnityNamespace
{
    [ManualDiInjectable]
    public class UnityTarget
    {
        public void Inject(string msg) {}
    }
}
";
            var (generatedCode, diagnostics) = GeneratorTestHelper.GenerateInjectable(code);
            Assert.That(diagnostics, Is.Empty);

            var codeList = new List<string>(generatedCode);
            var regFile = codeList.Find(x => x.Contains("ManualDi_Injection_Registration"));
            var injectorFile = codeList.Find(x => x.Contains("ManualDi_TestUnityNamespace_UnityTarget_Injector"));

            Assert.That(regFile, Is.Not.Null);
            Assert.That(regFile, Does.Contain("[UnityEngine.Scripting.Preserve]"));
            Assert.That(regFile, Does.Contain("[UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]"));
            Assert.That(regFile, Does.Contain("[System.Runtime.CompilerServices.ModuleInitializer]"));

            Assert.That(injectorFile, Is.Not.Null);
            Assert.That(injectorFile, Does.Contain("[UnityEngine.Scripting.Preserve]"));
        }

        // --- 5-Level Interleaved Inheritance: A (injected) -> B (not) -> C (injected) -> D (not) -> E (injected) ---

        [ManualDiInjectable]
        public class InterleavedA
        {
            public List<string> Sequence { get; } = new();

            public void Inject(string msg)
            {
                Sequence.Add($"A:{msg}");
            }
        }

        public class InterleavedB : InterleavedA
        {
            public void Inject(int num)
            {
                Sequence.Add($"B:{num}");
            }
        }

        [ManualDiInjectable]
        public class InterleavedC : InterleavedB
        {
            public new void Inject(int num)
            {
                Sequence.Add($"C:{num}");
            }
        }

        public class InterleavedD : InterleavedC
        {
        }

        [ManualDiInjectable]
        public class InterleavedE : InterleavedD
        {
            public void Inject(bool flag)
            {
                Sequence.Add($"E:{flag}");
            }
        }

        [Test]
        public async Task TestFiveLevelInterleavedInheritance()
        {
            var diContainer = await new DiContainerBindings().Install(b =>
            {
                b.Bind<string>().FromInstance("str");
                b.Bind<int>().FromInstance(42);
                b.Bind<bool>().FromInstance(true);
            }).Build(CancellationToken.None);
            await using var _ = diContainer;

            // InterleavedE should execute A, then C, then E (skipping B and D)
            var e = new InterleavedE();
            ManualDiInjector.Inject(e, diContainer);
            Assert.That(e.Sequence, Is.EqualTo(new[] { "A:str", "C:42", "E:True" }));

            // InterleavedD is not registered
            var d = new InterleavedD();
            Assert.That(ManualDiInjector.CanInject(d), Is.False);
            Assert.Throws<InvalidOperationException>(() => ManualDiInjector.Inject(d, diContainer));

            // InterleavedC should execute A, then C
            var c = new InterleavedC();
            ManualDiInjector.Inject(c, diContainer);
            Assert.That(c.Sequence, Is.EqualTo(new[] { "A:str", "C:42" }));

            // InterleavedB is not registered
            var bInstance = new InterleavedB();
            Assert.That(ManualDiInjector.CanInject(bInstance), Is.False);
            Assert.Throws<InvalidOperationException>(() => ManualDiInjector.Inject(bInstance, diContainer));

            // InterleavedA should execute A
            var a = new InterleavedA();
            ManualDiInjector.Inject(a, diContainer);
            Assert.That(a.Sequence, Is.EqualTo(new[] { "A:str" }));
        }

        // --- Generic with constraints ---

        public interface IServiceConstraint { }
        public class ServiceConstraintImpl : IServiceConstraint { }

        [ManualDiInjectable]
        public class GenericWithConstraint<T> where T : class, IServiceConstraint
        {
            public T? Service { get; private set; }

            public void Inject(T service)
            {
                Service = service;
            }
        }

        [ManualDiInjectable]
        public class ConcreteWithConstraint : GenericWithConstraint<ServiceConstraintImpl>
        {
            public string? Extra { get; private set; }

            public void Inject(string extra)
            {
                Extra = extra;
            }
        }

        [Test]
        public async Task TestGenericWithConstraint()
        {
            var impl = new ServiceConstraintImpl();
            var diContainer = await new DiContainerBindings().Install(b =>
            {
                b.Bind<ServiceConstraintImpl>().FromInstance(impl);
                b.Bind<string>().FromInstance("extra_data");
            }).Build(CancellationToken.None);
            await using var _ = diContainer;

            var concrete = new ConcreteWithConstraint();
            ManualDiInjector.Inject(concrete, diContainer);

            Assert.That(concrete.Service, Is.SameAs(impl));
            Assert.That(concrete.Extra, Is.EqualTo("extra_data"));
        }

        // --- Manual Registration ---

        public class CustomTarget
        {
            public int InjectedNumber { get; set; }
        }

        [Test]
        public async Task TestCustomManualRegistration()
        {
            var diContainer = await new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(789);
            }).Build(CancellationToken.None);
            await using var _ = diContainer;

            var custom = new CustomTarget();
            Assert.That(ManualDiInjector.CanInject(custom), Is.False);
            Assert.That(ManualDiInjector.CanInject(typeof(CustomTarget)), Is.False);

            ManualDiInjector.Register(typeof(CustomTarget), (target, container) =>
            {
                ((CustomTarget)target).InjectedNumber = container.Resolve<int>();
            });

            Assert.That(ManualDiInjector.CanInject(custom), Is.True);
            Assert.That(ManualDiInjector.CanInject(typeof(CustomTarget)), Is.True);

            ManualDiInjector.Inject(custom, diContainer);
            Assert.That(custom.InjectedNumber, Is.EqualTo(789));
        }
    }
}
