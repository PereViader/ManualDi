using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace ManualDi.Unity3d.Tests.PlayMode
{
    public class TestContainerStarterGameObjectPrefabExtensions
    {
        [UnityTest]
        public IEnumerator Destroy_ContextInstance_DisposesContainer()
        {
            bool disposed = false;

            var context = UnityManualDiTest.Instantiate(installDelegate: b =>
            {
                b.QueueDispose(() => disposed = true);
            });

            GameObject.Destroy(context.gameObject);

            yield return null;

            Assert.That(disposed, Is.True);
        }
    }
}
