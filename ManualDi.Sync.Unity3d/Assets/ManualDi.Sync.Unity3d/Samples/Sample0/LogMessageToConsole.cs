using ManualDi.Sync;
using UnityEngine;

namespace ManualD.Sync.Unity3d.Samples.Sample0
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