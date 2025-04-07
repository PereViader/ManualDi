using UnityEngine.UI;

namespace ManualDi.Sync.Unity3d.Samples.Ticking
{
    public class TickSample : MonoBehaviourRootEntryPoint
    {
        public Text Text;
        
        public override void Install(DiContainerBindings b)
        {
            // In order to be able to use the tickable, the service must be available
            b.Bind<ITickableService, TickableService>()
                .Default()
                .FromGameObjectAddComponent(gameObject)
                .LinkDontDestroyOnLoad();
            
            //Providing some text where to display values
            b.Bind<Text>().FromInstance(Text);
            
            // This is where we do the ticking. Wiring it to the service is as simple as calling LinkTickable
            b.Bind<IncreaseValueOnEachUpdate>()
                .Default()
                .FromConstructor()
                .LinkTickable(); 
        }
    }
}