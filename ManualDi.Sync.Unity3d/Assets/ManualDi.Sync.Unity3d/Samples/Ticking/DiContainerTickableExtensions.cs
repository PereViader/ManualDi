namespace ManualDi.Sync.Unity3d.Samples.Ticking
{
    public static class DiContainerTickableExtensions
    {
        public static Binding<TInterface, TConcrete> LinkTickable<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding
        )
            where TConcrete : ITickable
        {
            return binding.Inject((o, c) =>
            {
                var tickableService = c.Resolve<ITickableService>();
                tickableService.Add(o, TickType.Update);

                c.QueueDispose(() =>
                {
                    tickableService.Remove(o, TickType.Update);
                });
            });
        }
        
        public static Binding<TInterface, TConcrete> LinkTickable<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            TickType tickType
        )
            where TConcrete : ITickable
        {
            return binding.Inject((o, c) =>
            {
                var tickableService = c.Resolve<ITickableService>();
                tickableService.Add(o, tickType);
                c.QueueDispose(() =>
                {
                    tickableService.Remove(o, tickType);
                });
            });
        }
    }
}