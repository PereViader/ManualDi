using UnityEngine;

namespace ManualDi.Unity3d.Examples.Example2
{
    internal class Example2Context : MonoBehaviour
    {
        private PrimitiveType primitiveType;

        public void Inject(PrimitiveType primitiveType)
        {
            this.primitiveType = primitiveType;
        }

        public void Run()
        {
            GameObject.CreatePrimitive(primitiveType);
        }
    }
}
