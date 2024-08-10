using UnityEngine;

namespace ManualDi.Unity3d
{
    public static class GameObjectManualDi
    {
        public static TFacade Instantiate<TData, TFacade>(IContextEntryPoint<TData, TFacade> contextEntryPoint, TData data, IContextInitiator contextInitiator, Transform? parent = null)
            where TFacade : MonoBehaviour
        {
            var gameObjectInstance = GameObject.Instantiate(contextEntryPoint.GameObject, parent);
            var contextEntryPointInstance = gameObjectInstance.GetComponent<IContextEntryPoint<TData, TFacade>>();
            return contextInitiator.Initiate(contextEntryPointInstance, data);
        }
    }
}
