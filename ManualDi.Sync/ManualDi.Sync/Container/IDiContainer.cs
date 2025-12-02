using System;
using System.Collections;
using System.Threading;

namespace ManualDi.Sync
{
    public interface IDiContainer : IDisposable
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
        /// <param name="resolutions">The list of resolutions to be populated from the operations done inside. Resolutions are added at the end of the list</param>
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
    }
}
