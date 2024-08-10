using UnityEngine;

namespace ManualDi.Unity3d
{
    public interface IContextInitiator
    {
        TContext Initiate<TData, TContext>(IContextEntryPoint<TData, TContext> contextEntryPoint, TData data)
            where TContext : MonoBehaviour;
    }
}
