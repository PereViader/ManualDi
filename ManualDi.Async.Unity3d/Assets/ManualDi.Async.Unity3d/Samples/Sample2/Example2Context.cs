using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ManualDi.Async.Unity3d.Examples.Example2
{
    [ManualDi]
    internal class Example2Context : MonoBehaviour
    {
        private PrimitiveType _primitiveType;

        public void Inject(PrimitiveType primitiveType)
        {
            _primitiveType = primitiveType;
        }

        public async Task InitializeAsync(CancellationToken ct)
        {
            //Do some setup sync
            var instance = GameObject.CreatePrimitive(_primitiveType);
            
            await Task.Delay(1000, ct); // simulate some delay
            
            //Instantiate something async
            var secondInstance = GameObject.InstantiateAsync(instance);
            while (!secondInstance.isDone)
            {
                await Task.Yield();
            }
            secondInstance.Cancel();
        }

        public void Run()
        {
            Debug.Log("Example Run!!");
        }
    }
}
