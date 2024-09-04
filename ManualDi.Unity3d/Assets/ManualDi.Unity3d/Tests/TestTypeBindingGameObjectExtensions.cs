using NUnit.Framework;
using System.Collections;
using ManualDi.Main;
using UnityEngine;
using UnityEngine.TestTools;

namespace ManualDi.Unity3d.Tests.PlayMode
{
    public class TestContainerStarterGameObjectPrefabExtensions
    {
        private class TestSubordinateEntryPoint : SubordinateEntryPoint<TestInstallerData>
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

            var entryPoint = new GameObject().AddComponent<TestSubordinateEntryPoint>();
            
            entryPoint.Initiate(new TestInstallerData(b =>
            {
                b.QueueDispose(() => disposed = true);
            }));
        
            GameObject.Destroy(entryPoint.gameObject);
        
            yield return null;
        
            Assert.That(disposed, Is.True);
        }
    }
}
