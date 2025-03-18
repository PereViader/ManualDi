﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Main
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
        public Binding Binding = default!; // Optimization: we assume that it will be filled
        public Binding? InjectedIntoBinding;
    }

    public interface IDependencyResolver
    {
        void Dependency<T>();
        void Dependency<T>(FilterBindingDelegate filter);
    }
    
    public abstract class Binding
    {
        public abstract Type ApparentType { get; }
        public abstract Type ConcreteType { get; }
        
        internal bool TryToDispose = true;
        internal FilterBindingDelegate? FilterBindingDelegate;
        internal Action<IDependencyResolver>? Dependencies;
        internal object? Id;
        internal object? FromDelegate;
        internal object? InjectionDelegate;
        internal object? InitializationDelegate;
        
        internal bool IsAlreadyWired;
        internal Binding? NextBinding;
        internal object? Instance;
    }

    public sealed class Binding<TApparent, TConcrete> : Binding
    {
        public override Type ApparentType => typeof(TApparent);
        public override Type ConcreteType => typeof(TConcrete);
    }
}
