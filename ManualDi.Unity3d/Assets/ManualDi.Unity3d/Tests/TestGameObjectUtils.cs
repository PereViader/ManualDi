using JetBrains.Annotations;
using ManualDi.Main;
using UnityEngine;

namespace ManualDi.Unity3d.Tests.PlayMode
{
    public static class UnityManualDiTest
    {
        public static TestContext Instantiate(InstallDelegate installDelegate = null, IDiContainer parentDiContainer = null)
        {
            var gameObject = new GameObject();
            var contextEntryPoint = gameObject.AddComponent<TestSubEntryPoint>();

            contextEntryPoint.InstallDelegate = b =>
            {
                b.Bind<TestContext>()
                    .FromGameObjectAddComponent(gameObject)
                    .Initialize((o, c) =>
                    {
                        o.DiContainer = c;
                    });

                installDelegate?.Invoke(b);
            };

            return contextEntryPoint.Initiate(new object(), parentDiContainer);
        }
    }
}
