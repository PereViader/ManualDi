using ManualDi.Sync;
using UnityEngine;

namespace ManualD.Sync.Unity3d
{
    public abstract class MonoBehaviourInstaller : MonoBehaviour, IInstaller
    {
        public abstract void Install(DiContainerBindings b);
    }
}
