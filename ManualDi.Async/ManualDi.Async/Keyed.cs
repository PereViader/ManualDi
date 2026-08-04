namespace ManualDi.Async
{
    /// <summary>
    /// Type handle marker used strictly for Keyed binding dictionary lookups in IDiContainer.
    /// Never instantiated at runtime.
    /// </summary>
    public struct Keyed<TService, TKey>
        where TKey : struct
    {
    }
}
