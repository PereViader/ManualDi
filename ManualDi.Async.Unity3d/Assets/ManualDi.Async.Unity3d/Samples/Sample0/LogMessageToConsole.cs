using UnityEngine;

namespace ManualDi.Async.Unity3d.Samples.Sample0
{
    internal class LogMessageToConsole : MonoBehaviour
    {
        public string Message { get; set; }

        public void Inject(string message)
        {
            Message = message;
        }

        public void Initialize()
        {
            Debug.Log(Message);
        }
    }
}