using UnityEngine;

namespace ManualDi.Async.Unity3d.Examples.Example1
{
    [ManualDi]
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
