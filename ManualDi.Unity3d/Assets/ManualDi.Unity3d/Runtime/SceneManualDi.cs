using ManualDi.Unity3d.Utils;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManualDi.Unity3d
{
    public static class SceneManualDi
    {
        public static TContext Initiate<TContextEntryPoint, TData, TContext>(Scene scene, TData data, IContextInitiator contextInitiator)
            where TContextEntryPoint : IContextEntryPoint<TData, TContext>
            where TContext : MonoBehaviour
        {
            if (!UnitySceneUtils.TryFindComponentInSceneRoot<IContextEntryPoint<TData, TContext>>(scene, out var contextEntryPoint))
            {
                throw new InvalidOperationException($"Scene does not have root component of type {typeof(IContextEntryPoint<TData, TContext>).FullName}");
            }

            return contextInitiator.Initiate(contextEntryPoint, data);
        }
    }
}
