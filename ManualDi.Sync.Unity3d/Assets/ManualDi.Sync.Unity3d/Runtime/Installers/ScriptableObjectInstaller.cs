using ManualDi.Sync;
using UnityEngine;

namespace ManualD.Sync.Unity3d
{
    public abstract class ScriptableObjectInstaller : ScriptableObject, IInstaller
    {
        public abstract void Install(DiContainerBindings b);
    }
}
