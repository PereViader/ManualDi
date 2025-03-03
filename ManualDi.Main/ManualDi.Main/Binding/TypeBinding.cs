using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Main
{
    // Note: By having short parameter names on delegates, IDE autocomplete will create lambdas with these short names
    // We usually don't want abbreviated parameter names, but in this case because we are very likely to need to write
    // lots of lambdas, and the lambda parameters are always the same, short parameter names will be more readable
    public delegate T? CreateDelegate<out T>(IDiContainer c);
    public delegate Task<T?> CreateAsyncDelegate<T>(IDiContainer c, CancellationToken ct);
    public delegate void InstanceContainerDelegate<in T>(T o, IDiContainer c);
    public delegate Task InstanceContainerAsyncDelegate<in T>(T o, IDiContainer c, CancellationToken ct);
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

    public enum TypeScope
    {
        Single,
        Transient
    }
    
    public abstract class TypeBinding
    {
        public abstract Type ApparentType { get; }
        public abstract Type ConcreteType { get; }
        
        public TypeScope TypeScope = TypeScope.Single;
        public bool IsLazy = true;
        public bool TryToDispose = true;
        public object? Id;
        public FilterBindingDelegate? FilterBindingDelegate;
        internal object? SingleInstance;
        internal TypeBinding? NextTypeBinding;

        internal abstract object Resolve(DiContainer diContainer);
    }
}
