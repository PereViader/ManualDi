#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ManualDi.Sync.Unity3d
{
    [ManualDiInjectable]
    public sealed class ManualDiUnityEngineObjectInjector : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.Object[] objects = Array.Empty<UnityEngine.Object>();

        public UnityEngine.Object[] Objects
        {
            get => objects;
            set => objects = value ?? Array.Empty<UnityEngine.Object>();
        }

        public void Inject(IDiContainer diContainer)
        {
            foreach (var o in objects)
            {
                ManualDiInjector.Inject(o, diContainer);
            }
        }

#if UNITY_EDITOR
        public void PopulateInjectables()
        {
            var components = GetComponentsInChildren<Component>(true);
            var existingObjects = new HashSet<UnityEngine.Object>();
            var newObjects = new List<UnityEngine.Object>();

            if (objects != null)
            {
                for (var i = 0; i < objects.Length; i++)
                {
                    var o = objects[i];
                    if (o != null)
                    {
                        if (existingObjects.Add(o))
                        {
                            newObjects.Add(o);
                        }
                    }
                }
            }

            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component == null || component == this)
                {
                    continue;
                }

                if (!IsDirectParentInjector(component))
                {
                    continue;
                }

                var type = component.GetType();
                if (ManualDiInjector.CanInject(type) || type.IsDefined(typeof(ManualDiInjectableAttribute), true))
                {
                    if (existingObjects.Add(component))
                    {
                        newObjects.Add(component);
                    }
                }
            }

            objects = newObjects.ToArray();
        }

        private bool IsDirectParentInjector(Component component)
        {
            if (component is ManualDiUnityEngineObjectInjector otherInjector)
            {
                var parentTransform = otherInjector.transform.parent;
                if (parentTransform == null)
                {
                    return false;
                }

                return parentTransform.GetComponentInParent<ManualDiUnityEngineObjectInjector>(true) == this;
            }

            return component.GetComponentInParent<ManualDiUnityEngineObjectInjector>(true) == this;
        }
#endif
    }
}
