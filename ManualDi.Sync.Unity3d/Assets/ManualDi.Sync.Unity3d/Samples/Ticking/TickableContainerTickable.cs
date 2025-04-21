using System.Collections.Generic;

namespace ManualDi.Sync.Unity3d.Samples.Ticking
{
    public sealed class TickableContainerTickable : ITickable
    {
        readonly List<ITickable> _tickablesToAdd = new();
        readonly List<ITickable> _tickablesToRemove = new();

        readonly List<ITickable> _tickables = new();

        public void Tick()
        {
            ActuallyRemoveTickables();

            foreach (ITickable tickable in _tickables)
            {
                tickable.Tick();
            }

            ActuallyAddTickables();
        }

        /// <summary>
        /// Adds a tickable object to the container.
        /// </summary>
        /// <param name="tickable">The tickable object to add.</param>
        /// <exception cref="System.Exception">Thrown when the tickable is already added to the container.</exception>
        public void Add(ITickable tickable)
        {
            _tickablesToAdd.Add(tickable);
        }

        /// <summary>
        /// Removes a tickable object from the container.
        /// </summary>
        /// <param name="tickable">The tickable object to remove.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the tickable parameter is null.</exception>

        public void Remove(ITickable tickable)
        {
            bool contained = _tickables.Contains(tickable);

            if (!contained)
            {
                return;
            }
            
            _tickablesToRemove.Add(tickable);
        }

        /// <summary>
        /// Clears the container by removing all tickable objects.
        /// </summary>
        public void Clear()
        {
            _tickablesToRemove.Clear();
            _tickables.Clear();
        }

        void ActuallyAddTickables()
        {
            foreach(ITickable tickable in _tickablesToAdd)
            {
                _tickables.Add(tickable);
            }

            _tickablesToAdd.Clear();
        }

        void ActuallyRemoveTickables()
        {
            foreach (ITickable tickable in _tickablesToRemove)
            {
                _tickables.Remove(tickable);
            }

            _tickablesToRemove.Clear();
        }
    }
}