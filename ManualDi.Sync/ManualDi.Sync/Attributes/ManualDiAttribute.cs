using System;

namespace ManualDi.Sync
{
    /// <summary>
    /// Explicitly mark this class to be processed by the source generator.
    /// This attribute is required for the source generator to generate the Default/Inject/Initialize/Dispose binding methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ManualDiAttribute : Attribute
    {
    }
}