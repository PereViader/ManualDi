using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class DiContainerWouldResolveExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve<T>(this IDiContainer diContainer)
        {
            return diContainer.WouldResolveContainer(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve(this IDiContainer diContainer, Type type)
        {
            return diContainer.WouldResolveContainer(type);
        }
    }
}