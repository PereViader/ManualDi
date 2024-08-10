using UnityEngine;

namespace ManualDi.Unity3d.Examples.Example1
{
    public class Example1 : MonoBehaviour
    {
        public Example1ContextEntryPoint contextPrefab;

        private void Start()
        {
            // Instantiates the prefab, installs all the container bindings
            // and sets up the object graph
            var facade = GameObjectManualDi.Instantiate(contextPrefab, 5, RootContextInitiator.Instance);

            // You can now start using the gameobject system throught the facade
            facade.DoStuff();

            // Once the system is not needed, destroying the gameobject
            // Will dispose of everything
            GameObject.Destroy(facade.gameObject);
        }
    }
}
