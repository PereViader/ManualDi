using ManualDi.Main;

namespace ManualDi.Unity3d.Samples.Ticking
{
    public static class DiContainerTickableExtensions
    {
        public static TypeBinding<TInterface, TConcrete> LinkTickable<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding
        )
            where TConcrete : ITickable
        {
            return typeBinding.Inject((o, c) =>
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
        
        public static TypeBinding<TInterface, TConcrete> LinkTickable<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            TickType tickType
        )
            where TConcrete : ITickable
        {
            return typeBinding.Inject((o, c) =>
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