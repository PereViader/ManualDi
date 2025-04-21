using UnityEngine;

namespace ManualDi.Sync.Unity3d.Samples.Ticking
{
    public sealed class TickableService : MonoBehaviour, ITickableService
    {
        readonly TickableContainerTickable preUpdateTickable = new();
        readonly TickableContainerTickable updateTickable = new();
        readonly TickableContainerTickable lateUpdateTickable = new();
        readonly TickableContainerTickable fixedUpdateTickable = new();

        void Update()
        {
            preUpdateTickable.Tick();
            updateTickable.Tick();
        }

        void LateUpdate()
        {
            lateUpdateTickable.Tick();
        }

        void FixedUpdate()
        {
            fixedUpdateTickable.Tick();
        }

        public void Add(ITickable tickable, TickType tickType)
        {
            switch (tickType)
            {
                case TickType.PreUpdate:
                {
                    preUpdateTickable.Add(tickable);
                    break;
                }

                default:
                case TickType.Update:
                {
                    updateTickable.Add(tickable);
                    break;
                }

                case TickType.LateUpdate:
                {
                    lateUpdateTickable.Add(tickable);
                    break;
                }

                case TickType.FixedUpdate:
                {
                    fixedUpdateTickable.Add(tickable);
                    break;
                }
            }
        }

        public void Remove(ITickable tickable, TickType tickType)
        {
            switch (tickType)
            {
                case TickType.PreUpdate:
                {
                    preUpdateTickable.Remove(tickable);
                    break;
                }

                default:
                case TickType.Update:
                {
                    updateTickable.Remove(tickable);
                    break;
                }

                case TickType.LateUpdate:
                {
                    lateUpdateTickable.Remove(tickable);
                    break;
                }

                case TickType.FixedUpdate:
                {
                    fixedUpdateTickable.Remove(tickable);
                    break;
                }
            }
        }

        public void Clear()
        {
            preUpdateTickable.Clear();
            updateTickable.Clear();
            lateUpdateTickable.Clear();
            fixedUpdateTickable.Clear();
        }
    }
}