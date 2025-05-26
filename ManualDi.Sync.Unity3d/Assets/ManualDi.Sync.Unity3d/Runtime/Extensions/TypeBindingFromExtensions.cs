using UnityEngine;

namespace ManualDi.Sync.Unity3d
{
    public static class BindingUnity3dExtensions
    {
        #region From GameObject
        
        public static Binding<TInterface, TConcrete> FromGameObjectGetComponent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            GameObject gameObject
        )
            where TConcrete : Component
        {
            binding.FromMethod(_ => gameObject.GetComponent<TConcrete>());
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> FromGameObjectGetComponentInChildren<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            GameObject gameObject
        )
            where TConcrete : Component
        {
            binding.FromMethod(_ => gameObject.GetComponentInChildren<TConcrete>());
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> FromGameObjectGetComponentInParent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            GameObject gameObject
        )
            where TConcrete : Component
        {
            binding.FromMethod(_ => gameObject.GetComponentInParent<TConcrete>());
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> FromGameObjectAddComponent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            GameObject gameObject,
            bool destroyOnDispose = true
            )
            where TConcrete : Component
        {
            binding.FromMethod(c =>
            {
                var o = gameObject.AddComponent<TConcrete>();
                if (destroyOnDispose)
                {
                    c.QueueDispose(() => Object.Destroy(o));
                }
                return o;
            });
            
            return binding;
        }
        
        #endregion

        #region From Instantiate Component

        public static Binding<TInterface, TConcrete> FromInstantiateComponent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            TConcrete component,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            binding.FromMethod(c =>
            {
                var o = Object.Instantiate(component, parent, worldPositionStays);
                if (destroyOnDispose)
                {
                    c.QueueDispose(() => Object.Destroy(o.gameObject));
                }

                return o;
            });
            return binding;
        }

        #endregion
        
        #region From Instantiate GameObject
        
        public static Binding<TInterface, TConcrete> FromInstantiateGameObjectGetComponent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            GameObject gameObject,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            binding.FromMethod(c =>
            {
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                var o = instance.GetComponent<TConcrete>();
                if (destroyOnDispose)
                {
                    c.QueueDispose(() => Object.Destroy(instance));
                }
                return o;
            });
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> FromInstantiateGameObjectGetComponentInChildren<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            GameObject gameObject,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            binding.FromMethod(c =>
            {
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                var o = instance.GetComponentInChildren<TConcrete>();
                if (destroyOnDispose)
                {
                    c.QueueDispose(() =>
                    {
                        Object.Destroy(instance);
                    });
                }
                return o;
            });
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> FromInstantiateGameObjectAddComponent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            GameObject gameObject,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            binding.FromMethod(c =>
            {
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                var o = instance.AddComponent<TConcrete>();
                if (destroyOnDispose)
                {
                    c.QueueDispose(() =>
                    {
                        Object.Destroy(instance);
                    });
                }
                return o;
            });
            return binding;
        }
        
        #endregion
        
        #region From Object Resource

        public static Binding<TInterface, TConcrete> FromObjectResource<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            string path
        )
            where TConcrete : UnityEngine.Object
        {
            binding.FromMethod(_ => Resources.Load<TConcrete>(path));
            return binding;
        }

        #endregion
        
        #region From Instantiate GameObject Resource
        
        public static Binding<TInterface, TConcrete> FromInstantiateGameObjectResourceGetComponent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            string path,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            binding.FromMethod(c =>
            {
                var gameObject = Resources.Load<GameObject>(path);
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                
                if (destroyOnDispose)
                {
                    c.QueueDispose(() =>
                    {
                        Object.Destroy(instance);
                    });
                }
                
                return instance.GetComponent<TConcrete>();
            });
            
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> FromInstantiateGameObjectResourceGetComponentInChildren<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            string path,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            GameObject? instance = null;
            binding.FromMethod(c =>
            {
                var gameObject = Resources.Load<GameObject>(path);
                instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                
                if (destroyOnDispose)
                {
                    c.QueueDispose(() => Object.Destroy(instance));
                }
                
                return instance.GetComponentInChildren<TConcrete>();
            });
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> FromInstantiateGameObjectResourceAddComponent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            string path,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            binding.FromMethod(c =>
            {
                var gameObject = Resources.Load<GameObject>(path);
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);

                if (destroyOnDispose)
                {
                    c.QueueDispose(() => Object.Destroy(instance));
                }
                
                return instance.AddComponent<TConcrete>();
            });
            return binding;
        }
        
        #endregion
    }
}
