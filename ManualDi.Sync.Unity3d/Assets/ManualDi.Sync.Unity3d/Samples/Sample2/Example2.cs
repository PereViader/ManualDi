using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManualDi.Sync.Unity3d.Examples.Example2
{
    internal class Example2 : MonoBehaviour
    {
        public PrimitiveType primitiveType;

        private void Start()
        {
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            // Load a scene that has a context
            yield return SceneManager.LoadSceneAsync("Example2Context", LoadSceneMode.Additive);
            
            // Get the entry point that was loaded from the scene, do this in any way you want
            var entryPoint = Object.FindObjectOfType<Example2EntryPoint>();
            
            //Create the container and get the context object out
            var context = entryPoint.Initiate(primitiveType);

            // Start using the scene context
            context.Run();
        }
    }
}
