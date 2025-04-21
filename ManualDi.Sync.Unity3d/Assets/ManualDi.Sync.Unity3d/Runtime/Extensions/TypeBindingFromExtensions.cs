using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ManualDi.Sync.Unity3d
{
    public static class BindingUnity3dExtensions
    {
        #region From GameObject
        
        public static Binding<TInterface, TConcrete> FromGameObjectGetComponentInParent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            GameObject gameObject
        )
            where TConcrete : Component
        {
            binding.FromMethod(_ => gameObject.GetComponentInParent<TConcrete>());
            return binding;
        }
        
        public static Binding<List<TInterface>, List<TConcrete>> FromGameObjectGetComponentsInParent<TInterface, TConcrete>(
            this Binding<List<TInterface>, List<TConcrete>> binding,
            GameObject gameObject,
            bool allowEmpty = false
        )
            where TConcrete : Component
        {
            return binding.FromMethod(_ =>
            {
                var componentsInParent = gameObject.GetComponentsInParent<TConcrete>();
                if (componentsInParent.Length == 0 && !allowEmpty)
                {
                    return null;
                }
                return componentsInParent.ToList();
            });
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

        public static Binding<List<TInterface>, List<TConcrete>> FromGameObjectGetComponentsInChildren<TInterface, TConcrete>(
            this Binding<List<TInterface>, List<TConcrete>> binding,
            GameObject gameObject,
            bool allowEmpty = false
            )
            where TConcrete : Component
        {
            binding.FromMethod(_ =>
            {
                var componentsInParent = gameObject.GetComponentsInChildren<TConcrete>();
                if (componentsInParent.Length == 0 && !allowEmpty)
                {
                    return null;
                }
                return componentsInParent.ToList();
            });
            return binding;
        }

        public static Binding<TInterface, TConcrete> FromGameObjectGetComponent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            GameObject gameObject
            )
            where TConcrete : Component
        {
            binding.FromMethod(_ => gameObject.GetComponent<TConcrete>());
            return binding;
        }

        public static Binding<TInterface, TConcrete> FromGameObjectAddComponent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            GameObject gameObject,
            bool destroyOnDispose = true
            )
            where TConcrete : Component
        {
            binding.FromMethod(_ => gameObject.AddComponent<TConcrete>());
            if (destroyOnDispose)
            {
                binding.Dispose((o, c) => Object.Destroy(o));
            }
            return binding;
        }

        public static Binding<List<TInterface>, List<TConcrete>> FromGameObjectGetComponents<TInterface, TConcrete>(
            this Binding<List<TInterface>, List<TConcrete>> binding, 
            GameObject gameObject,
            bool allowEmpty = false
            )
            where TConcrete : Component
        {
            binding.FromMethod(_ =>
            {
                var components = gameObject.GetComponents<TConcrete>();
                if (components.Length == 0 && !allowEmpty)
                {
                    return null;
                }
                return components.ToList();
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
            binding.FromMethod(_ => Object.Instantiate(component, parent, worldPositionStays));
            if (destroyOnDispose)
            {
                binding.Dispose((o, c) => Object.Destroy(o.gameObject));
            }
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
            binding.FromMethod(_ =>
            {
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                return instance.GetComponent<TConcrete>();
            });
            if (destroyOnDispose)
            {
                binding.Dispose((o, c) => Object.Destroy(o.gameObject));
            }
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
            GameObject? instance = null; 
            binding.FromMethod(_ =>
            {
                instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                return instance.GetComponentInChildren<TConcrete>();
            });
            if (destroyOnDispose)
            {
                binding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
            return binding;
        }
        
        public static Binding<List<TInterface>, List<TConcrete>> FromInstantiateGameObjectGetComponents<TInterface, TConcrete>(
            this Binding<List<TInterface>, List<TConcrete>> binding,
            GameObject gameObject,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool allowEmpty = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            GameObject? instance = null;
            binding.FromMethod(_ =>
            {
                instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                var components = instance.GetComponents<TConcrete>();
                if (components.Length == 0 && !allowEmpty)
                {
                    Object.Destroy(instance);
                    instance = null;
                    return null;
                }
                return components.ToList();
            });
            if (destroyOnDispose)
            {
                binding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
            return binding;
        }
        
        public static Binding<List<TInterface>, List<TConcrete>> FromInstantiateGameObjectGetComponentsInChildren<TInterface, TConcrete>(
            this Binding<List<TInterface>, List<TConcrete>> binding,
            GameObject gameObject,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool allowEmpty = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            GameObject? instance = null;
            binding.FromMethod(_ =>
            {
                instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                var components = instance.GetComponentsInChildren<TConcrete>();
                if (components.Length == 0 && !allowEmpty)
                {
                    Object.Destroy(instance);
                    instance = null;
                    return null;
                }
                return components.ToList();
            });
            if (destroyOnDispose)
            {
                binding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
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
            binding.FromMethod(_ =>
            {
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                return instance.AddComponent<TConcrete>();
            });
            if (destroyOnDispose)
            {
                binding.Dispose((o, c) => Object.Destroy(o.gameObject));
            }
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
            binding.FromMethod(_ =>
            {
                var gameObject = Resources.Load<GameObject>(path);
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                return instance.GetComponent<TConcrete>();
            });
            if (destroyOnDispose)
            {
                binding.Dispose((o, c) => Object.Destroy(o.gameObject));
            }
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
            binding.FromMethod(_ =>
            {
                var gameObject = Resources.Load<GameObject>(path);
                instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                return instance.GetComponentInChildren<TConcrete>();
            });
            if (destroyOnDispose)
            {
                binding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
            return binding;
        }
        
        public static Binding<List<TInterface>, List<TConcrete>> FromInstantiateGameObjectResourceGetComponents<TInterface, TConcrete>(
            this Binding<List<TInterface>, List<TConcrete>> binding,
            string path,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool allowEmpty = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            GameObject? instance = null;
            binding.FromMethod(_ =>
            {
                var gameObject = Resources.Load<GameObject>(path);
                instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                var components = instance.GetComponents<TConcrete>();
                if (components.Length == 0 && !allowEmpty)
                {
                    Object.Destroy(instance);
                    instance = null;
                    return null;
                }
                return components.ToList();
            });
            
            if (destroyOnDispose)
            {
                binding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
            return binding;
        }
        
        public static Binding<List<TInterface>, List<TConcrete>> FromInstantiateGameObjectResourceGetComponentsInChildren<TInterface, TConcrete>(
            this Binding<List<TInterface>, List<TConcrete>> binding,
            string path,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool allowEmpty = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            GameObject? instance = null;
            binding.FromMethod(_ =>
            {
                var gameObject = Resources.Load<GameObject>(path);
                instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                var components = instance.GetComponentsInChildren<TConcrete>();
                if (components.Length == 0 && !allowEmpty)
                {
                    Object.Destroy(instance);
                    instance = null;
                    return null;
                }
                return components.ToList();
            });
            
            if (destroyOnDispose)
            {
                binding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
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
            binding.FromMethod(_ =>
            {
                var gameObject = Resources.Load<GameObject>(path);
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                return instance.AddComponent<TConcrete>();
            });
            if (destroyOnDispose)
            {
                binding.Dispose((o, c) => Object.Destroy(o.gameObject));
            }
            return binding;
        }
        
        #endregion
        
    }
}
