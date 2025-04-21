using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Async
{
    public static class BindingInjectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> Inject<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding, 
            InjectDelegate injectionDelegate)
        {
            binding.InjectionDelegate = binding.InjectionDelegate is null 
                ? injectionDelegate 
                : binding.InjectionDelegate switch
                {
                    InjectDelegate existingSync => existingSync + injectionDelegate,
                    InjectAsyncDelegate existingAsync => (InjectAsyncDelegate)(async (o, c, ct) =>
                    {
                        await existingAsync(o, c, ct);
                        injectionDelegate(o, c);
                    }),
                    _ => throw new InvalidOperationException()
                };
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> InjectAsync<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding, 
            InjectAsyncDelegate injectionAsyncDelegate)
        {
            binding.InjectionDelegate = binding.InjectionDelegate is null 
                ? injectionAsyncDelegate 
                : binding.InjectionDelegate switch
                {
                    InjectDelegate existingSync => (InjectAsyncDelegate)(async (o, c, ct) =>
                    {
                        existingSync(o, c);
                        await injectionAsyncDelegate(o, c, ct);
                    }),
                    InjectAsyncDelegate existingAsync => (InjectAsyncDelegate)(async (o, c, ct) =>
                    {
                        await existingAsync(o, c, ct);
                        await injectionAsyncDelegate(o, c, ct);
                    }),
                    _ => throw new InvalidOperationException()
                };
            return binding;
        }
    }
}
