using UnityEngine;

namespace ManualDi.Unity3d
{
    public class RootContextInitiator : IContextInitiator
    {
        public static RootContextInitiator Instance { get; } = new();

        private RootContextInitiator()
        {
        }

        public TFacade Initiate<TData, TFacade>(IContextEntryPoint<TData, TFacade> contextEntryPoint, TData data) where TFacade : MonoBehaviour
        {
            return contextEntryPoint.Initiate(parentDiContainer: null, data);
        }
    }
}
