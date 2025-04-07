using NUnit.Framework;
using System.Collections;
using System.Threading;
using ManualDi.Async;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ManualDi.Unity3d.Tests.PlayMode
{
    public class TestContainerStarterGameObjectPrefabExtensions
    {
        private class TestMonoBehaviourSubordinateEntryPoint : MonoBehaviourSubordinateEntryPoint<TestInstallerData>
        {
            public override void Install(DiContainerBindings b)
            {
            }
        }

        private class TestInstallerData : IInstaller
        {
            public TestInstallerData(InstallDelegate installDelegate)
            {
                InstallDelegate = installDelegate;
            }

            public InstallDelegate InstallDelegate { get; }
            
            public void Install(DiContainerBindings bindings)
            {
                InstallDelegate.Invoke(bindings);
            }
        }
        
        [UnityTest]
        public IEnumerator Destroy_ContextInstance_DisposesContainer()
        {
            bool disposed = false;

            var entryPoint = new GameObject().AddComponent<TestMonoBehaviourSubordinateEntryPoint>();
            
            entryPoint.Initiate(new TestInstallerData(b =>
            {
                b.QueueDispose(() => disposed = true);
            }), CancellationToken.None);
        
            GameObject.Destroy(entryPoint.gameObject);
        
            yield return null;
        
            Assert.That(disposed, Is.True);
        }

        [UnityTest]
        public IEnumerator FromGameObjectAddComponent_AddsComponentThenRemovesIt()
        {
            var gameObject = new GameObject();
            
            var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<Image>().FromGameObjectAddComponent(gameObject);
            }).Build(CancellationToken.None).Result;
            
            Assert.That(gameObject.GetComponent<Image>(), Is.Not.Null);
            
            container.DisposeAsync();

            yield return null;
            
            Assert.That(gameObject.GetComponent<Image>(), Is.Null);
        }
        
        [Test]
        public void FromGameObjectGetComponent_GetsComponent()
        {
            var gameObject = new GameObject();
            var image = gameObject.AddComponent<Image>();
            
            var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<Image>().FromGameObjectGetComponent(gameObject);
            }).Build(CancellationToken.None).Result;
            
            Assert.That(image, Is.EqualTo(container.Resolve<Image>()));

            container.DisposeAsync();
        }
        
        [Test]
        public void FromGameObjectAddComponent_InstantiatesAndGetsComponent()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<Image>();
            
            var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<Image>().FromInstantiateGameObjectGetComponent(gameObject, parent: gameObject.transform);
            }).Build(CancellationToken.None).Result;

            var containerInstance = container.Resolve<Image>();
            var instance = gameObject.transform.GetChild(0).GetComponent<Image>();
            Assert.That(instance, Is.EqualTo(containerInstance));
            
            container.DisposeAsync();
        }
    }
}
