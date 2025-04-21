namespace ManualDi.Async.Unity3d.Samples.Ticking
{
    public interface ITickableService
    {
        /// <summary>
        /// Adds a tickable object to the service.
        /// </summary>
        /// <param name="tickable">The tickable object to add.</param>
        /// <param name="tickType">When the tick is going to be performed.</param>
        void Add(ITickable tickable, TickType tickType);

        /// <summary>
        /// Removes a tickable object from the service.
        /// </summary>
        /// <param name="tickable">The tickable object to remove.</param>
        /// <param name="tickType">When the tick is was being performed.</param>
        void Remove(ITickable tickable, TickType tickType);
        
        void Clear();
    }
}