using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Async
{
    public interface IDiContainer : IDependencyResolver, IAsyncDisposable // Only use IDisposable if you are certain that there are no IAsyncDisposables registered
    {
        CancellationToken CancellationToken { get; }
        
        /// <summary>
        /// Non-generic resolution of a binding for its registered instance
        /// </summary>
        /// <remarks>There are more versions of this Resolution using extension methods</remarks>
        /// <param name="type">The type of the binding to resolve</param>
        /// <returns>The resolved instance for the binding</returns>
        object? ResolveContainer(Type type);
        
        /// <summary>
        /// Non-generic resolution of a binding for its registered instance
        /// </summary>
        /// <remarks>There are more versions of this Resolution using extension methods</remarks>
        /// <param name="type">The type of the binding to resolve</param>
        /// <param name="filterBindingDelegate">Filter bindings to resolve according to these constraints. May be null</param>
        /// <returns>The resolved instance for the binding</returns>
        object? ResolveContainer(Type type, FilterBindingDelegate filterBindingDelegate);

        /// <summary>
        /// Non-generic resolution of all bindings for their registered instances
        /// </summary>
        /// <remarks>There are more versions of this Resolution using extension methods</remarks>
        /// <param name="type">The type of the binding to resolve</param>
        /// <param name="filterBindingDelegate">Filter bindings to resolve according to these constraints. May be null</param>
        /// <param name="resolutions">The list of resolutions to be populated from the operations done inside. Resolutions are added at the end of the list</param>
        /// <returns>The resolved instances for the binding</returns>
        void ResolveAllContainer(Type type, FilterBindingDelegate? filterBindingDelegate, IList resolutions);

        /// <summary>
        /// Checks if calling ResolveContainer or ResolveAllContainer would return any instance.
        /// False positives are possible if the bindings would at creation time
        /// </summary>
        /// <param name="type">The type of the binding to resolve</param>
        /// <param name="filterBindingDelegate">Filter bindings to resolve according to these constraints. May be null</param>
        /// <param name="overrideInjectedIntoType">If left null this will simulate a root resolve. If a type is set, it will simulate a resolve as if it was resolved from that type</param>
        /// <param name="overrideFilterBindingDelegate"></param>
        /// <returns>True when resolving would succeed or throw, false when resolving returns null or empty list</returns>
        bool WouldResolveContainer(
            Type type, 
            FilterBindingDelegate? filterBindingDelegate, 
            Type? overrideInjectedIntoType, 
            FilterBindingDelegate? overrideFilterBindingDelegate);
        
        /// <summary>
        /// Queues for disposal a disposable. They will be called in order when disposing the container.
        /// </summary>
        /// <param name="disposable">The disposable to queue</param>
        void QueueDispose(IDisposable disposable);
        
        /// <summary>
        /// Queues for disposal an action. They will be called in order when disposing the container.
        /// </summary>
        /// <param name="disposableAction">The disposable action to queue</param>
        void QueueDispose(Action disposableAction);
        
        /// <summary>
        /// Queues for disposal an AsyncDisposable. They will be called in order when disposing the container.
        /// </summary>
        /// <param name="asyncDisposable">The disposable func to queue</param>
        void QueueAsyncDispose(IAsyncDisposable asyncDisposable);
        
        /// <summary>
        /// Queues for disposal a disposable Func. They will be called in order when disposing the container.
        /// </summary>
        /// <param name="asyncDisposableFunc">The disposable func to queue</param>
        void QueueAsyncDispose(Func<ValueTask> asyncDisposableFunc);
    }
}
