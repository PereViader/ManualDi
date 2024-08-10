using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManualDi.Unity3d.Examples.Example2
{
    public class Example2 : MonoBehaviour
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

            // Get a reference to the scene that was loaded
            Scene scene = SceneManager.GetSceneByName("Example2Context");

            // Initiate the scene context giving it the data it needs
            Example2Context context = SceneManualDi.Initiate<Example2ContextEntryPoint, PrimitiveType, Example2Context>(
                scene,
                primitiveType,
                RootContextInitiator.Instance
                );

            // Start using the scene context
            context.Run();
        }
    }
}
