using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManualDi.Async.Unity3d.Examples.Example2
{
    internal class Example2 : MonoBehaviour
    {
        public PrimitiveType primitiveType;

        private async void Start()
        {
            // Load a scene that has a context
            var asyncOperation = SceneManager.LoadSceneAsync("Example2Context", LoadSceneMode.Additive);
            while (!asyncOperation!.isDone)
            {
                await Task.Yield();
            }
            
            // Get the entry point that was loaded from the scene, do this in any way you want
            var entryPoint = Object.FindObjectOfType<Example2EntryPoint>();
            
            //Create the container and get the context object out
            var context = await entryPoint.Initiate(primitiveType, CancellationToken.None);

            // Start using the scene context
            context.Run();
        }
    }
}
