using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Async
{
    public static class ManualDiInjector
    {
        private static readonly Dictionary<IntPtr, Action<object, IDiContainer>> Injectors = new Dictionary<IntPtr, Action<object, IDiContainer>>();

        public static void Register(Type type, Action<object, IDiContainer> injector)
        {
            Injectors[type.TypeHandle.Value] = injector;
        }

        public static bool CanInject(Type type)
        {
            return Injectors.ContainsKey(type.TypeHandle.Value);
        }

        public static bool CanInject(object target)
        {
            return Injectors.ContainsKey(target.GetType().TypeHandle.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Inject(object target, IDiContainer container)
        {
            if (Injectors.TryGetValue(target.GetType().TypeHandle.Value, out var injector))
            {
                injector(target, container);
                return;
            }

            ThrowHelper.ThrowTypeNotRegisteredForInjection(target.GetType());
        }

        public static void Clear()
        {
            Injectors.Clear();
        }
    }
}
