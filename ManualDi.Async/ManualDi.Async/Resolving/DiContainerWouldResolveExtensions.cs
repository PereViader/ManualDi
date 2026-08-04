using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ManualDi.Async
{
    public static class DiContainerWouldResolveExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve<T>(this IDiContainer diContainer)
        {
            return diContainer.WouldResolveContainer(typeof(T));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolveAsync<T>(this IDiContainer diContainer)
        {
            return diContainer.WouldResolveContainer(typeof(Task<T>));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve(this IDiContainer diContainer, Type type)
        {
            return diContainer.WouldResolveContainer(type);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolveAsync(this IDiContainer diContainer, Type type)
        {
            return diContainer.WouldResolveContainer(typeof(Task<>).MakeGenericType(type));
        }
    }
}