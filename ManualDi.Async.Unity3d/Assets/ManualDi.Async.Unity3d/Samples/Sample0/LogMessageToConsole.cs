using UnityEngine;

namespace ManualDi.Async.Unity3d.Samples.Sample0
{
    internal class LogMessageToConsole : MonoBehaviour
    {
        [Inject] public string Message { get; set; }

        public void Initialize()
        {
            Debug.Log(Message);
        }
    }
}