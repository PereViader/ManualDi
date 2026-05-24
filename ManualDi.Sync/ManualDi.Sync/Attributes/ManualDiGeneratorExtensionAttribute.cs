using System;

namespace ManualDi.Sync
{
    /// <summary>
    /// Add to a static extension method to automatically call it on matching generated bindings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ManualDiGeneratorExtensionAttribute : Attribute
    {
    }
}
