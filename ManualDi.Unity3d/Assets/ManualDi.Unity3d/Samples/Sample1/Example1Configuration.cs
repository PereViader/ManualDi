using UnityEngine;

namespace ManualDi.Unity3d.Examples.Example1
{
    [CreateAssetMenu(menuName = "Examples/Example1/Configuration")]
    public class Example1Configuration : ScriptableObject
    {
        public string message = default;
    }
}
