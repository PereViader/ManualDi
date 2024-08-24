#if UNITY_EDITOR

using ManualDi.Main;
using UnityEngine;

namespace ManualDi.Unity3d.Tests
{
    [AddComponentMenu("")]
    public class TestContext : MonoBehaviour
    {
        public IDiContainer DiContainer { get; set; } = default!;
    }
}

#endif
