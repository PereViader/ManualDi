using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Main
{
    public interface ITypeBindingAsyncSetup
    {
        ValueTask CreateAsync(DiContainer diContainer);
        ValueTask InjectAsync(DiContainer diContainer);
        ValueTask InitializeAsync(CancellationToken cancellationToken);
    }

    public sealed class TypeBindingAsync<TApparent, TConcrete> : TypeBinding, ITypeBindingAsyncSetup
    {
        public override Type ApparentType => typeof(TApparent);
        public override Type ConcreteType => typeof(TConcrete);

        public FromDelegate<TConcrete>? CreateDelegate;
        public FromAsyncDelegate<TConcrete>? CreateAsyncDelegate;
        public InjectDelegate<TConcrete>? InjectionDelegate;
        public InjectAsyncDelegate<TConcrete>? InjectionAsyncDelegate;
        public InitializeDelegate<TConcrete>? InitializationDelegate;
        public InitializeAsyncDelegate<TConcrete>? InitializationAsyncDelegate;

        async ValueTask ITypeBindingAsyncSetup.CreateAsync(DiContainer diContainer)
        {
            Instance = (CreateDelegate, CreateAsyncDelegate) switch
            {
                (not null, not null) => throw new InvalidOperationException($"TypeBindingAsync with Apparent type {typeof(TApparent)} and Concrete type {typeof(TConcrete)} has both sync and async creation delegates. It should only have one."),
                (null, null) => throw new InvalidOperationException($"TypeBindingAsync with Apparent type {typeof(TApparent)} and Concrete type {typeof(TConcrete)} has no creation delegates defined."),
                (not null, null) => CreateDelegate.Invoke(diContainer) ?? throw new InvalidOperationException($"Could not create object for TypeBindingAsync with Apparent type {typeof(TApparent)} and Concrete type {typeof(TConcrete)}"),
                (null, not null) => await CreateAsyncDelegate.Invoke(diContainer, diContainer.CancellationToken) ?? throw new InvalidOperationException($"Could not create object for TypeBindingAsync with Apparent type {typeof(TApparent)} and Concrete type {typeof(TConcrete)}"),
            };
        }

        async ValueTask ITypeBindingAsyncSetup.InjectAsync(DiContainer diContainer)
        {
            var instance = (TConcrete)Instance!;
            if (InjectionAsyncDelegate is not null)
            {
                await InjectionAsyncDelegate.Invoke(instance, diContainer, diContainer.CancellationToken);
            }
            InjectionDelegate?.Invoke(instance, diContainer);
        }

        async ValueTask ITypeBindingAsyncSetup.InitializeAsync(CancellationToken cancellationToken)
        {
            var instance = (TConcrete)Instance!;
            if (InitializationAsyncDelegate is not null)
            {
                await InitializationAsyncDelegate.Invoke(instance, cancellationToken);
            }
            InitializationDelegate?.Invoke(instance);
        }
    }
}