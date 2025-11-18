using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Async
{
    // Note: By having short parameter names on delegates, IDE autocomplete will create lambdas with these short names
    // We usually don't want abbreviated parameter names, but in this case because we are very likely to need to write
    // lots of lambdas, and the lambda parameters are always the same, short parameter names will be more readable
    public delegate object? FromDelegate(IDiContainer c);
    public delegate Task<object?> FromAsyncDelegate(IDiContainer c, CancellationToken ct);
    public delegate void InjectDelegate(object o, IDiContainer c);
    public delegate Task InjectAsyncDelegate(object o, IDiContainer c, CancellationToken ct);
    public delegate void InitializeDelegate(object o);
    public delegate Task InitializeAsyncDelegate(object o, CancellationToken ct);
    public delegate void DisposeObjectDelegate(object o);
    public delegate Action DisposeObjectContextDelegate(object o, IDiContainer c);
    public delegate ValueTask AsyncDisposeObjectDelegate(object o);
    public delegate Func<ValueTask> AsyncDisposeObjectContextDelegate(object o, IDiContainer c);
    
    public delegate bool FilterBindingDelegate(BindingContext context);

    public sealed class BindingContext
    {
        public Binding Binding = null!; // Optimization: we assume that it will be filled
        public Binding? InjectedIntoBinding;
    }

    public interface IDependencyResolver
    {
        void ConstructorDependency<T>();
        void ConstructorDependency<T>(FilterBindingDelegate filter);
        void NullableConstructorDependency<T>();
        void NullableConstructorDependency<T>(FilterBindingDelegate filter);
        void InjectionDependency<T>();
        void InjectionDependency<T>(FilterBindingDelegate filter);
        void NullableInjectionDependency<T>();
        void NullableInjectionDependency<T>(FilterBindingDelegate filter);
    }

    internal enum BindingWiredState
    {
        None,
        Injected,
        Wired
    }
    
    public abstract class Binding
    {
        public abstract Type ConcreteType { get; }
        
        internal bool TryToDispose = true;
        internal FilterBindingDelegate? FilterBindingDelegate;
        internal Action<IDependencyResolver>? Dependencies;
        internal object? Id;
        internal object? FromDelegate; //Contains either FromDelegate or FromAsyncDelegate or an instance of type TConcrete
        internal object? InjectionDelegate;
        internal object? InitializationDelegate;
        
        internal BindingWiredState BindingWiredState; // TODO: Check if it can be updated to bool
        internal Binding? NextBinding;
        internal object? Instance;
    }

    public sealed class Binding<TConcrete> : Binding
    {
        public override Type ConcreteType => typeof(TConcrete);
    }
}
