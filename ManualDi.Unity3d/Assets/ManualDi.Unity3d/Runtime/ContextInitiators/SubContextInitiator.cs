using ManualDi.Main;
using UnityEngine;

namespace ManualDi.Unity3d
{
    public class SubContextInitiator : IContextInitiator
    {
        private readonly IDiContainer _parentContainer;

        public SubContextInitiator(IDiContainer parentContainer)
        {
            _parentContainer = parentContainer;
        }

        public TFacade Initiate<TData, TFacade>(IContextEntryPoint<TData, TFacade> contextEntryPoint, TData data) where TFacade : MonoBehaviour
        {
            return contextEntryPoint.Initiate(_parentContainer, data);
        }
    }
}
