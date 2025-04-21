using UnityEngine;

namespace ManualDi.Async.Unity3d
{
    public abstract class MonoBehaviourInstaller : MonoBehaviour, IInstaller
    {
        public abstract void Install(DiContainerBindings b);
    }
}
