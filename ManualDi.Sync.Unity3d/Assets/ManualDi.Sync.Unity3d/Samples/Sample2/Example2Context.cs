using UnityEngine;

namespace ManualDi.Sync.Unity3d.Examples.Example2
{
    internal class Example2Context : MonoBehaviour
    {
        private PrimitiveType _primitiveType;

        public void Inject(PrimitiveType primitiveType)
        {
            _primitiveType = primitiveType;
        }

        public void Run()
        {
            GameObject.CreatePrimitive(_primitiveType);
        }
    }
}
