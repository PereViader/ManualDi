
namespace ManualDi.Async.Unity3d.Examples.Example1
{
    internal class Example1EntryPoint : MonoBehaviourSubordinateEntryPoint<int, Example1Context>
    {
        public Example1Configuration configuration;
        public Example1Context context;

        public override void Install(DiContainerBindings b)
        {
            b.Bind<Example1Context>().Default().FromInstance(context);
            b.Bind<Example1Configuration>().FromInstance(configuration);
            b.Bind<int>().FromInstance(Data);

            b.QueueDispose(() => UnityEngine.Debug.Log("Dispose " + Data));
        }
    }
}
