#nullable enable
using System;
using NUnit.Framework;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ManualDi.Sync.Unity3d.Tests
{
    public class TestManualDiUnityEngineObjectInjector
    {
        [ManualDiInjectable]
        public class InjectableMonoBehaviourA : MonoBehaviour
        {
            public string? Text { get; private set; }
            public int Number { get; private set; }

            public void Inject(string text, int number)
            {
                Text = text;
                Number = number;
            }
        }

        [ManualDiInjectable]
        public class InjectableMonoBehaviourB : MonoBehaviour
        {
            public IDiContainer? Container { get; private set; }

            public void Inject(IDiContainer container)
            {
                Container = container;
            }
        }

        public class NonInjectableMonoBehaviour : MonoBehaviour
        {
            public string? Text { get; set; }

            public void Inject(string text)
            {
                Text = text;
            }
        }

        [ManualDiInjectable]
        public class InjectableScriptableObject : ScriptableObject
        {
            public string? Text { get; private set; }

            public void Inject(string text)
            {
                Text = text;
            }
        }

        private IDiContainer diContainer = null!;
        private GameObject rootGameObject = null!;

        [SetUp]
        public void SetUp()
        {
            diContainer = new DiContainerBindings().Install(b =>
            {
                b.Bind<string>().FromInstance("injected_string");
                b.Bind<int>().FromInstance(99);
            }).Build();

            rootGameObject = new GameObject("RootTestObject");
        }

        [TearDown]
        public void TearDown()
        {
            diContainer?.Dispose();
            if (rootGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(rootGameObject);
            }
        }

        [Test]
        public void Inject_WithInjectableMonoBehaviours_InjectsAll()
        {
            var injector = rootGameObject.AddComponent<ManualDiUnityEngineObjectInjector>();
            var compA = rootGameObject.AddComponent<InjectableMonoBehaviourA>();
            var compB = rootGameObject.AddComponent<InjectableMonoBehaviourB>();

            injector.Objects = new UnityEngine.Object[] { compA, compB };
            injector.Inject(diContainer);

            Assert.That(compA.Text, Is.EqualTo("injected_string"));
            Assert.That(compA.Number, Is.EqualTo(99));
            Assert.That(compB.Container, Is.SameAs(diContainer));
        }

        [Test]
        public void Inject_WithInjectableScriptableObject_InjectsDependencies()
        {
            var injector = rootGameObject.AddComponent<ManualDiUnityEngineObjectInjector>();
            var so = ScriptableObject.CreateInstance<InjectableScriptableObject>();

            try
            {
                injector.Objects = new UnityEngine.Object[] { so };
                injector.Inject(diContainer);

                Assert.That(so.Text, Is.EqualTo("injected_string"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(so);
            }
        }

        [Test]
        public void Inject_ViaManualDiInjector_InjectsInjector()
        {
            var injector = rootGameObject.AddComponent<ManualDiUnityEngineObjectInjector>();
            var compA = rootGameObject.AddComponent<InjectableMonoBehaviourA>();

            injector.Objects = new UnityEngine.Object[] { compA };
            ManualDiInjector.Inject(injector, diContainer);

            Assert.That(compA.Text, Is.EqualTo("injected_string"));
            Assert.That(compA.Number, Is.EqualTo(99));
        }

        [Test]
        public void PopulateInjectables_FindsInjectableComponentsInHierarchy()
        {
            var injector = rootGameObject.AddComponent<ManualDiUnityEngineObjectInjector>();
            var compA = rootGameObject.AddComponent<InjectableMonoBehaviourA>();
            rootGameObject.AddComponent<NonInjectableMonoBehaviour>();

            var child1 = new GameObject("Child1");
            child1.transform.SetParent(rootGameObject.transform);
            var compB = child1.AddComponent<InjectableMonoBehaviourB>();

            var child2 = new GameObject("Child2Inactive");
            child2.transform.SetParent(rootGameObject.transform);
            child2.SetActive(false);
            var compChildA = child2.AddComponent<InjectableMonoBehaviourA>();

            injector.PopulateInjectables();

            Assert.That(injector.Objects, Is.EquivalentTo(new UnityEngine.Object[] { compA, compB, compChildA }));
        }

        [Test]
        public void PopulateInjectables_ExcludesNonInjectableAndInjectorItself()
        {
            var injector = rootGameObject.AddComponent<ManualDiUnityEngineObjectInjector>();
            var nonInjectable = rootGameObject.AddComponent<NonInjectableMonoBehaviour>();

            injector.PopulateInjectables();

            CollectionAssert.DoesNotContain(injector.Objects, injector);
            CollectionAssert.DoesNotContain(injector.Objects, nonInjectable);
            Assert.That(injector.Objects, Is.Empty);
        }

        [Test]
        public void PopulateInjectables_PreservesExistingObjectsAndAppendsNewToTail()
        {
            var injector = rootGameObject.AddComponent<ManualDiUnityEngineObjectInjector>();
            var compA = rootGameObject.AddComponent<InjectableMonoBehaviourA>();

            var child = new GameObject("Child");
            child.transform.SetParent(rootGameObject.transform);
            var compB = child.AddComponent<InjectableMonoBehaviourB>();

            var so = ScriptableObject.CreateInstance<InjectableScriptableObject>();

            try
            {
                injector.Objects = new UnityEngine.Object[] { so, compA };
                injector.PopulateInjectables();

                Assert.That(injector.Objects.Length, Is.EqualTo(3));
                Assert.That(injector.Objects[0], Is.SameAs(so));
                Assert.That(injector.Objects[1], Is.SameAs(compA));
                Assert.That(injector.Objects[2], Is.SameAs(compB));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(so);
            }
        }

        [Test]
        public void PopulateInjectables_WhenCalledMultipleTimes_DoesNotDuplicate()
        {
            var injector = rootGameObject.AddComponent<ManualDiUnityEngineObjectInjector>();
            var compA = rootGameObject.AddComponent<InjectableMonoBehaviourA>();

            var child = new GameObject("Child");
            child.transform.SetParent(rootGameObject.transform);
            var compB = child.AddComponent<InjectableMonoBehaviourB>();

            injector.PopulateInjectables();
            var countAfterFirstCall = injector.Objects.Length;
            Assert.That(countAfterFirstCall, Is.EqualTo(2));

            injector.PopulateInjectables();
            Assert.That(injector.Objects.Length, Is.EqualTo(2));
            Assert.That(injector.Objects[0], Is.SameAs(compA));
            Assert.That(injector.Objects[1], Is.SameAs(compB));
        }

        [ManualDiInjectable]
        public class CountingMonoBehaviour : MonoBehaviour
        {
            public int InjectionCount { get; private set; }

            public void Inject(string text)
            {
                InjectionCount++;
            }
        }

        [Test]
        public void PopulateInjectables_WithNestedChildInjector_IncludesChildInjectorAndExcludesChildsChildren()
        {
            var rootInjector = rootGameObject.AddComponent<ManualDiUnityEngineObjectInjector>();
            var compRoot = rootGameObject.AddComponent<InjectableMonoBehaviourA>();

            var childGo = new GameObject("SubRoot");
            childGo.transform.SetParent(rootGameObject.transform);
            var childInjector = childGo.AddComponent<ManualDiUnityEngineObjectInjector>();
            var compChild = childGo.AddComponent<InjectableMonoBehaviourB>();

            var grandChildGo = new GameObject("GrandChild");
            grandChildGo.transform.SetParent(childGo.transform);
            var compGrandChild = grandChildGo.AddComponent<InjectableMonoBehaviourA>();

            rootInjector.PopulateInjectables();
            childInjector.PopulateInjectables();

            CollectionAssert.AreEqual(new UnityEngine.Object[] { compRoot, childInjector }, rootInjector.Objects);
            CollectionAssert.AreEqual(new UnityEngine.Object[] { compChild, compGrandChild }, childInjector.Objects);
        }

        [Test]
        public void Inject_WithNestedInjectors_InjectsEachComponentExactlyOnce()
        {
            var rootInjector = rootGameObject.AddComponent<ManualDiUnityEngineObjectInjector>();
            var compRoot = rootGameObject.AddComponent<CountingMonoBehaviour>();

            var childGo = new GameObject("SubRoot");
            childGo.transform.SetParent(rootGameObject.transform);
            var childInjector = childGo.AddComponent<ManualDiUnityEngineObjectInjector>();
            var compChild = childGo.AddComponent<CountingMonoBehaviour>();

            var grandChildGo = new GameObject("GrandChild");
            grandChildGo.transform.SetParent(childGo.transform);
            var compGrandChild = grandChildGo.AddComponent<CountingMonoBehaviour>();

            rootInjector.PopulateInjectables();
            childInjector.PopulateInjectables();

            rootInjector.Inject(diContainer);

            Assert.That(compRoot.InjectionCount, Is.EqualTo(1));
            Assert.That(compChild.InjectionCount, Is.EqualTo(1));
            Assert.That(compGrandChild.InjectionCount, Is.EqualTo(1));
        }

        [Test]
        public void PopulateInjectables_DeeplyNestedInjectors_ProperlySegmentsHierarchy()
        {
            var rootInjector = rootGameObject.AddComponent<ManualDiUnityEngineObjectInjector>();
            var comp0 = rootGameObject.AddComponent<CountingMonoBehaviour>();

            var level1Go = new GameObject("Level1");
            level1Go.transform.SetParent(rootGameObject.transform);
            var injector1 = level1Go.AddComponent<ManualDiUnityEngineObjectInjector>();
            var comp1 = level1Go.AddComponent<CountingMonoBehaviour>();

            var level2Go = new GameObject("Level2");
            level2Go.transform.SetParent(level1Go.transform);
            var injector2 = level2Go.AddComponent<ManualDiUnityEngineObjectInjector>();
            var comp2 = level2Go.AddComponent<CountingMonoBehaviour>();

            var level3Go = new GameObject("Level3");
            level3Go.transform.SetParent(level2Go.transform);
            var comp3 = level3Go.AddComponent<CountingMonoBehaviour>();

            rootInjector.PopulateInjectables();
            injector1.PopulateInjectables();
            injector2.PopulateInjectables();

            CollectionAssert.AreEqual(new UnityEngine.Object[] { comp0, injector1 }, rootInjector.Objects);
            CollectionAssert.AreEqual(new UnityEngine.Object[] { comp1, injector2 }, injector1.Objects);
            CollectionAssert.AreEqual(new UnityEngine.Object[] { comp2, comp3 }, injector2.Objects);

            rootInjector.Inject(diContainer);

            Assert.That(comp0.InjectionCount, Is.EqualTo(1));
            Assert.That(comp1.InjectionCount, Is.EqualTo(1));
            Assert.That(comp2.InjectionCount, Is.EqualTo(1));
            Assert.That(comp3.InjectionCount, Is.EqualTo(1));
        }

#if UNITY_EDITOR
        [Test]
        public void CustomEditor_CanBeCreated()
        {
            var injector = rootGameObject.AddComponent<ManualDiUnityEngineObjectInjector>();
            rootGameObject.AddComponent<InjectableMonoBehaviourA>();

            var editor = Editor.CreateEditor(injector);
            try
            {
                Assert.That(editor, Is.Not.Null);
                Assert.That(editor, Is.InstanceOf<ManualDiUnityEngineObjectInjectorEditor>());
                Assert.That(editor.target, Is.SameAs(injector));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(editor);
            }
        }
#endif
    }
}
