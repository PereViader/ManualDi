using ManualDi.Main;

namespace ManualDi.Unity3d.Examples.Example1
{
    internal class Example1EntryPoint : SubordinateEntryPoint<int, Example1Context>
    {
        public Example1Configuration configuration;
        public Example1Context context;

        public override void Install(DiContainerBindings b)
        {
            b.Bind<Example1Context>()
                .FromInstance(context)
                .Initialize((o, c) => o.Inject(Data, configuration));

            b.QueueDispose(() => UnityEngine.Debug.Log("Dispose " + Data));
        }
    }
}
