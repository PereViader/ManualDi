#if UNITY_EDITOR

using ManualDi.Main;
using UnityEngine;

namespace ManualDi.Unity3d.Tests
{
    [AddComponentMenu("")]
    public class TestContextEntryPoint : BaseContextEntryPoint<object, TestContext>
    {
        public InstallDelegate InstallDelegate { get; set; } = _ => { };

        public override void Install(DiContainerBindings bindings)
        {
            InstallDelegate.Invoke(bindings);
        }
    }
}
#endif
