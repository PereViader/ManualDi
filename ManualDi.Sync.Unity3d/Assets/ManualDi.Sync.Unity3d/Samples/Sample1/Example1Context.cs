using UnityEngine;

namespace ManualDi.Sync.Unity3d.Examples.Example1
{
    internal class Example1Context : MonoBehaviour
    {
        private int number;
        private Example1Configuration configuration;

        public void Inject(int number, Example1Configuration configuration)
        {
            this.number = number;
            this.configuration = configuration;
        }

        public void DoStuff()
        {
            Debug.LogFormat("{0} {1}", configuration.message, number);
        }
    }
}
