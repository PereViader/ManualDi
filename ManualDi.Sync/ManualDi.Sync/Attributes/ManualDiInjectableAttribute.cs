using System;

namespace ManualDi.Sync
{
    /// <summary>
    /// Explicitly mark this class as injectable for ManualDiInjector.
    /// This attribute triggers source generation of an optimized injector that invokes the Inject method
    /// and automatically registers the type for O(1) runtime injection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ManualDiInjectableAttribute : Attribute
    {
    }
}
