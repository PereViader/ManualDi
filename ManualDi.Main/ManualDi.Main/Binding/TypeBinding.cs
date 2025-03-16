using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Main
{
    // Note: By having short parameter names on delegates, IDE autocomplete will create lambdas with these short names
    // We usually don't want abbreviated parameter names, but in this case because we are very likely to need to write
    // lots of lambdas, and the lambda parameters are always the same, short parameter names will be more readable
    public delegate T? FromDelegate<out T>(IDiContainer c);
    public delegate Task<T?> FromAsyncDelegate<T>(IDiContainer c, CancellationToken ct);
    public delegate void InjectDelegate<in T>(T o, IDiContainer c);
    public delegate Task InjectAsyncDelegate<in T>(T o, IDiContainer c, CancellationToken ct);
    public delegate void InitializeDelegate<in T>(T o);
    public delegate Task InitializeAsyncDelegate<in T>(T o, CancellationToken ct);
    public delegate void DisposeObjectDelegate<in T>(T o);
    public delegate Action DisposeObjectContextDelegate<in T>(T o, IDiContainer c);
    public delegate ValueTask AsyncDisposeObjectDelegate<in T>(T o);
    public delegate Func<ValueTask> AsyncDisposeObjectContextDelegate<in T>(T o, IDiContainer c);
    
    public delegate bool FilterBindingDelegate(BindingContext context);

    public sealed class BindingContext
    {
        public TypeBinding TypeBinding = default!; // Optimization: we assume that it will be filled
        public TypeBinding? InjectedIntoTypeBinding;
    }

    public interface IDependencyResolver
    {
        void Dependency<T>();
        void Dependency<T>(FilterBindingDelegate filter);
    }
    
    public abstract class TypeBinding
    {
        public abstract Type ApparentType { get; }
        public abstract Type ConcreteType { get; }
        
        internal object? Instance;
        public Action<IDependencyResolver>? Dependencies = default!;
        public TypeBinding[] BindingDependencies = default!;
        public bool TryToDispose = true;
        public object? Id;
        public FilterBindingDelegate? FilterBindingDelegate;
        internal TypeBinding? NextTypeBinding;
        internal bool IsAlreadyWired;

        internal IEnumerable<TypeBinding> GetChildBindings()
        {
            var binding = this;
            while (binding is not null)
            {
                yield return binding;
                binding = binding.NextTypeBinding;
            }
        }
    }
}
