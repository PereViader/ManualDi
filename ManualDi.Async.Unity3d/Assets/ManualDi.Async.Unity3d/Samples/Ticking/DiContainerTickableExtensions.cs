
namespace ManualDi.Async.Unity3d.Samples.Ticking
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
                var to = (ITickable)o;
                var tickableService = c.Resolve<ITickableService>();
                tickableService.Add(to, TickType.Update);

                c.QueueDispose(() =>
                {
                    tickableService.Remove(to, TickType.Update);
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
                var to = (ITickable)o;

                var tickableService = c.Resolve<ITickableService>();
                tickableService.Add(to, tickType);

                c.QueueDispose(() =>
                {
                    tickableService.Remove(to, tickType);
                });
            });
        }
    }
}