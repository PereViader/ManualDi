using UnityEngine;
using Object = UnityEngine.Object;

namespace ManualDi.Async.Unity3d
{
    //This class is shared between the Sync and Async packages
    public static class BindingUnity3dSyncExtensions
    {
        #region From GameObject
        
        public static Binding<TConcrete> FromGameObjectGetComponent<TConcrete>(
            this Binding<TConcrete> binding,
            GameObject gameObject
        )
            where TConcrete : Component
        {
            binding.FromMethod(_ => gameObject.GetComponent<TConcrete>());
            return binding;
        }
        
        public static Binding<TConcrete> FromGameObjectGetComponentInChildren<TConcrete>(
            this Binding<TConcrete> binding,
            GameObject gameObject
        )
            where TConcrete : Component
        {
            binding.FromMethod(_ => gameObject.GetComponentInChildren<TConcrete>());
            return binding;
        }
        
        public static Binding<TConcrete> FromGameObjectGetComponentInParent<TConcrete>(
            this Binding<TConcrete> binding,
            GameObject gameObject
        )
            where TConcrete : Component
        {
            binding.FromMethod(_ => gameObject.GetComponentInParent<TConcrete>());
            return binding;
        }
        
        public static Binding<TConcrete> FromGameObjectAddComponent<TConcrete>(
            this Binding<TConcrete> binding,
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

        public static Binding<TConcrete> FromInstantiateComponent<TConcrete>(
            this Binding<TConcrete> binding,
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
        
        public static Binding<TConcrete> FromInstantiateGameObjectGetComponent<TConcrete>(
            this Binding<TConcrete> binding,
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
        
        public static Binding<TConcrete> FromInstantiateGameObjectGetComponentInChildren<TConcrete>(
            this Binding<TConcrete> binding,
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
        
        public static Binding<TConcrete> FromInstantiateGameObjectAddComponent<TConcrete>(
            this Binding<TConcrete> binding,
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

        public static Binding<TConcrete> FromObjectResource<TConcrete>(
            this Binding<TConcrete> binding,
            string path
        )
            where TConcrete : UnityEngine.Object
        {
            binding.FromMethod(_ => Resources.Load<TConcrete>(path));
            return binding;
        }

        #endregion
        
        #region From Instantiate GameObject Resource
        
        public static Binding<TConcrete> FromInstantiateGameObjectResourceGetComponent<TConcrete>(
            this Binding<TConcrete> binding,
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
        
        public static Binding<TConcrete> FromInstantiateGameObjectResourceGetComponentInChildren<TConcrete>(
            this Binding<TConcrete> binding,
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
        
        public static Binding<TConcrete> FromInstantiateGameObjectResourceAddComponent<TConcrete>(
            this Binding<TConcrete> binding,
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
