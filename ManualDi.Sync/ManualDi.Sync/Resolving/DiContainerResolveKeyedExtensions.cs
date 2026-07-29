using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class DiContainerResolveKeyedExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TApparent ResolveKeyed<TApparent, TKey>(this IDiContainer diContainer)
            where TKey : struct
        {
            return (TApparent)diContainer.ResolveContainer(typeof(Keyed<TApparent, TKey>))!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TApparent? ResolveKeyedNullable<TApparent, TKey>(this IDiContainer diContainer)
            where TApparent : class
            where TKey : struct
        {
            return (TApparent?)diContainer.ResolveContainer(typeof(Keyed<TApparent, TKey>));
        }
    }
}
