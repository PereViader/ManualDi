using UnityEngine;

namespace ManualDi.Sync.Unity3d
{
    public abstract class MonoBehaviourInstaller : MonoBehaviour, IInstaller
    {
        public abstract void Install(DiContainerBindings b);
    }
}
