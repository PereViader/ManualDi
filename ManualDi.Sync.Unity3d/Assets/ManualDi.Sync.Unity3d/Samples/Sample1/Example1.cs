using UnityEngine;

namespace ManualDi.Sync.Unity3d.Examples.Example1
{
    internal class Example1 : MonoBehaviour
    {
        public Example1EntryPoint entryPointPrefab;

        private void Start()
        {
            var entryPointInstance = Object.Instantiate(entryPointPrefab);
            
            var facade = entryPointInstance.Initiate(5);

            // You can now start using the gameobject system throught the facade
            facade.DoStuff();

            // Once the system is not needed, destroying the gameobject
            // Will dispose of everything
            Object.Destroy(entryPointInstance.gameObject);
        }
    }
}
