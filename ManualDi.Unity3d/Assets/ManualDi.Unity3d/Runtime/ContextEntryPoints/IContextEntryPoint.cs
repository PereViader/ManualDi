using ManualDi.Main;
using System;
using UnityEngine;

namespace ManualDi.Unity3d
{
    public interface IContextEntryPoint<TData, out TContext> : IDisposable
        where TContext : MonoBehaviour
    {
        GameObject GameObject { get; }
        IDiContainer? Container { get; }
        TContext? Context { get; }
        TData? Data { get; }

        TContext Initiate(IDiContainer? parentDiContainer, TData data);
    }
}
