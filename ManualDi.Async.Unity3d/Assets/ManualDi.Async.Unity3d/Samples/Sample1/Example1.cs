using System.Threading;
using UnityEngine;

namespace ManualDi.Async.Unity3d.Examples.Example1
{
    internal class Example1 : MonoBehaviour
    {
        public Example1EntryPoint entryPointPrefab;

        private async void Start()
        {
            var entryPointInstance = Object.Instantiate(entryPointPrefab);
            
            var facade = await entryPointInstance.Initiate(5, CancellationToken.None);

            // You can now start using the gameobject system throught the facade
            facade.DoStuff();

            // Once the system is not needed, destroying the gameobject
            // Will dispose of everything
            Object.Destroy(entryPointInstance.gameObject);
        }
    }
}
