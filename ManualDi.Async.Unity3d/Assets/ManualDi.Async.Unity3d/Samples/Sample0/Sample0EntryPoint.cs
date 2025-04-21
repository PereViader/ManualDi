
namespace ManualDi.Async.Unity3d.Samples.Sample0
{
    internal class Sample0EntryPoint : MonoBehaviourRootEntryPoint
    {
        public string message;
        
        public override void Install(DiContainerBindings b)
        {
            b.Bind<string>().FromInstance(message);
            b.Bind<LogMessageToConsole>().Default().FromGameObjectAddComponent(gameObject);
        }
    }
}