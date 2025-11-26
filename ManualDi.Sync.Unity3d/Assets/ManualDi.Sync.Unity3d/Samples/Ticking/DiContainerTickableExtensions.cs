namespace ManualDi.Sync.Unity3d.Samples.Ticking
{
    public static class DiContainerTickableExtensions
    {
        public static Binding<TConcrete> LinkTickable<TConcrete>(
            this Binding<TConcrete> binding
        )
            where TConcrete : ITickable
        {
            return binding.Inject((o, c) =>
            {
                var tickableService = c.Resolve<ITickableService>();
                tickableService.Add((ITickable)o, TickType.Update);

                c.QueueDispose(() =>
                {
                    tickableService.Remove((ITickable)o, TickType.Update);
                });
            });
        }
        
        public static Binding<TConcrete> LinkTickable<TConcrete>(
            this Binding<TConcrete> binding,
            TickType tickType
        )
            where TConcrete : ITickable
        {
            return binding.Inject((o, c) =>
            {
                var tickableService = c.Resolve<ITickableService>();
                tickableService.Add((ITickable)o, tickType);
                c.QueueDispose(() =>
                {
                    tickableService.Remove((ITickable)o, tickType);
                });
            });
        }
    }
}