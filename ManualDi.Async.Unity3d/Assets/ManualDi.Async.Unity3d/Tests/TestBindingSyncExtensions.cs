using NUnit.Framework;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ManualDi.Async.Unity3d.Tests.PlayMode
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
            
            yield return TestExtensions.Async(async ct =>
            {
                await entryPoint.Initiate(new TestInstallerData(b =>
                {
                    b.QueueDispose(() => disposed = true);
                }), ct);
            });
        
            GameObject.Destroy(entryPoint.gameObject);
        
            yield return null;
        
            Assert.That(disposed, Is.True);
        }

        [UnityTest]
        public IEnumerator FromGameObjectAddComponent_AddsComponentThenRemovesIt()
        {
            var gameObject = new GameObject();
            
            yield return TestExtensions.Async(async ct =>
            {
                await using var container = await new DiContainerBindings().Install(b =>
                {
                    b.Bind<Image>().FromGameObjectAddComponent(gameObject);
                }).Build(ct);
                
                Assert.That(gameObject.GetComponent<Image>(), Is.Not.Null);
            });

            yield return null;
            
            Assert.That(gameObject.GetComponent<Image>(), Is.Null);
        }
        
        [Test]
        public async Task FromGameObjectGetComponent_GetsComponent()
        {
            var gameObject = new GameObject();
            var image = gameObject.AddComponent<Image>();
            
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<Image>().FromGameObjectGetComponent(gameObject);
            }).Build(CancellationToken.None);
            
            Assert.That(image, Is.EqualTo(container.Resolve<Image>()));
        }
        
        [Test]
        public async Task FromGameObjectAddComponent_InstantiatesAndGetsComponent()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<Image>();
            
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<Image>().FromInstantiateGameObjectGetComponent(gameObject, parent: gameObject.transform);
            }).Build(CancellationToken.None);

            var containerInstance = container.Resolve<Image>();
            var instance = gameObject.transform.GetChild(0).GetComponent<Image>();
            Assert.That(instance, Is.EqualTo(containerInstance));
        }
    }
}
