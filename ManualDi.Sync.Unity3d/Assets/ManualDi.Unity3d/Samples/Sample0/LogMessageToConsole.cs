using ManualDi.Main;
using UnityEngine;

namespace ManualDi.Unity3d.Samples.Sample0
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