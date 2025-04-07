using ManualDi.Async;
using UnityEngine;

namespace ManualDi.Unity3d
{
    public abstract class MonoBehaviourInstaller : MonoBehaviour, IInstaller
    {
        public abstract void Install(DiContainerBindings b);
    }
}
