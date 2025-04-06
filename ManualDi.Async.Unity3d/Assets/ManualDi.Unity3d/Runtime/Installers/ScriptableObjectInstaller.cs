using ManualDi.Main;
using UnityEngine;

namespace ManualDi.Unity3d
{
    public abstract class ScriptableObjectInstaller : ScriptableObject, IInstaller
    {
        public abstract void Install(DiContainerBindings b);
    }
}
