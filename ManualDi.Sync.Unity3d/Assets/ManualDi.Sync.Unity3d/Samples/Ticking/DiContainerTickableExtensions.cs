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
                })
                .Dispose((o, c) =>
                {
                    var tickableService = c.Resolve<ITickableService>();
                    tickableService.Remove(o, TickType.Update);
                })
                .NonLazy();
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
                })
                .Dispose((o, c) =>
                {
                    var tickableService = c.Resolve<ITickableService>();
                    tickableService.Remove(o, tickType);
                })
                .NonLazy();
        }
    }
}