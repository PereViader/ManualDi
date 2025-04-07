using UnityEngine;

namespace ManualDi.Sync.Unity3d.Examples.Example1
{
    [CreateAssetMenu(menuName = "Examples/Example1/Configuration")]
    internal class Example1Configuration : ScriptableObject
    {
        public string message = default;
    }
}
