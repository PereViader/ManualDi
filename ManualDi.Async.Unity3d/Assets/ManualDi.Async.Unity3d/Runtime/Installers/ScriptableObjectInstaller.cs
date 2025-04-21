using UnityEngine;

namespace ManualDi.Async.Unity3d
{
    public abstract class ScriptableObjectInstaller : ScriptableObject, IInstaller
    {
        public abstract void Install(DiContainerBindings b);
    }
}
