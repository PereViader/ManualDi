using ManualDi.Main;

namespace ManualDi.Unity3d.Examples.Example1
{
    public class Example1ContextEntryPoint : BaseContextEntryPoint<int, Example1Context>
    {
        public Example1Configuration configuration;
        public Example1Context context;

        public override void Install(DiContainerBindings bindings)
        {
            bindings.Bind<Example1Context>()
                .FromInstance(context)
                .Initialize((o, c) => o.Inject(Data, configuration));

            bindings.QueueDispose(() => UnityEngine.Debug.Log("Dispose " + Data));
        }
    }
}
