using System.Collections.Generic;
using System.Linq;
using ManualDi.Main;

using UnityEngine;

namespace ManualDi.Unity3d
{
    public static class TypeBindingUnity3dExtensions
    {
        #region From GameObject
        
        public static TypeBinding<TInterface, TConcrete> FromGameObjectGetComponentInParent<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            GameObject gameObject
        )
            where TConcrete : Component
        {
            typeBinding.FromMethod(_ => gameObject.GetComponentInParent<TConcrete>());
            return typeBinding;
        }
        
        public static TypeBinding<List<TInterface>, List<TConcrete>> FromGameObjectGetComponentsInParent<TInterface, TConcrete>(
            this TypeBinding<List<TInterface>, List<TConcrete>> typeBinding,
            GameObject gameObject,
            bool allowEmpty = false
        )
            where TConcrete : Component
        {
            return typeBinding.FromMethod(_ =>
            {
                var componentsInParent = gameObject.GetComponentsInParent<TConcrete>();
                if (componentsInParent.Length == 0 && !allowEmpty)
                {
                    return null;
                }
                return componentsInParent.ToList();
            });
        }
        
        public static TypeBinding<TInterface, TConcrete> FromGameObjectGetComponentInChildren<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            GameObject gameObject
            )
            where TConcrete : Component
        {
            typeBinding.FromMethod(_ => gameObject.GetComponentInChildren<TConcrete>());
            return typeBinding;
        }

        public static TypeBinding<List<TInterface>, List<TConcrete>> FromGameObjectGetComponentsInChildren<TInterface, TConcrete>(
            this TypeBinding<List<TInterface>, List<TConcrete>> typeBinding,
            GameObject gameObject,
            bool allowEmpty = false
            )
            where TConcrete : Component
        {
            typeBinding.FromMethod(_ =>
            {
                var componentsInParent = gameObject.GetComponentsInChildren<TConcrete>();
                if (componentsInParent.Length == 0 && !allowEmpty)
                {
                    return null;
                }
                return componentsInParent.ToList();
            });
            return typeBinding;
        }

        public static TypeBinding<TInterface, TConcrete> FromGameObjectGetComponent<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            GameObject gameObject
            )
            where TConcrete : Component
        {
            typeBinding.FromMethod(_ => gameObject.GetComponent<TConcrete>());
            return typeBinding;
        }

        public static TypeBinding<TInterface, TConcrete> FromGameObjectAddComponent<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            GameObject gameObject,
            bool destroyOnDispose = true
            )
            where TConcrete : Component
        {
            typeBinding.FromMethod(_ => gameObject.AddComponent<TConcrete>());
            if (destroyOnDispose)
            {
                typeBinding.Dispose((o, c) => Object.Destroy(o));
            }
            return typeBinding;
        }

        public static TypeBinding<List<TInterface>, List<TConcrete>> FromGameObjectGetComponents<TInterface, TConcrete>(
            this TypeBinding<List<TInterface>, List<TConcrete>> typeBinding, 
            GameObject gameObject,
            bool allowEmpty = false
            )
            where TConcrete : Component
        {
            typeBinding.FromMethod(_ =>
            {
                var components = gameObject.GetComponents<TConcrete>();
                if (components.Length == 0 && !allowEmpty)
                {
                    return null;
                }
                return components.ToList();
            });
            return typeBinding;
        }
        
        #endregion

        #region From Instantiate Component

        public static TypeBinding<TInterface, TConcrete> FromInstantiateComponent<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            TConcrete component,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            typeBinding.FromMethod(_ => Object.Instantiate(component, parent, worldPositionStays));
            if (destroyOnDispose)
            {
                typeBinding.Dispose((o, c) => Object.Destroy(o.gameObject));
            }
            return typeBinding;
        }

        #endregion
        
        #region From Instantiate GameObject
        
        public static TypeBinding<TInterface, TConcrete> FromInstantiateGameObjectGetComponent<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            GameObject gameObject,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            typeBinding.FromMethod(_ =>
            {
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                return instance.GetComponent<TConcrete>();
            });
            if (destroyOnDispose)
            {
                typeBinding.Dispose((o, c) => Object.Destroy(o.gameObject));
            }
            return typeBinding;
        }
        
        public static TypeBinding<TInterface, TConcrete> FromInstantiateGameObjectGetComponentInParent<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            GameObject gameObject,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            GameObject? instance = null;
            typeBinding.FromMethod(_ =>
            {
                instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                return instance.GetComponentInParent<TConcrete>();
            });
            if (destroyOnDispose)
            {
                typeBinding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
            return typeBinding;
        }
        
        public static TypeBinding<TInterface, TConcrete> FromInstantiateGameObjectGetComponentInChildren<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            GameObject gameObject,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            GameObject? instance = null; 
            typeBinding.FromMethod(_ =>
            {
                instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                return instance.GetComponentInChildren<TConcrete>();
            });
            if (destroyOnDispose)
            {
                typeBinding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
            return typeBinding;
        }
        
        public static TypeBinding<List<TInterface>, List<TConcrete>> FromInstantiateGameObjectGetComponents<TInterface, TConcrete>(
            this TypeBinding<List<TInterface>, List<TConcrete>> typeBinding,
            GameObject gameObject,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool allowEmpty = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            GameObject? instance = null;
            typeBinding.FromMethod(_ =>
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
                typeBinding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
            return typeBinding;
        }
        
        public static TypeBinding<List<TInterface>, List<TConcrete>> FromInstantiateGameObjectGetComponentsInParent<TInterface, TConcrete>(
            this TypeBinding<List<TInterface>, List<TConcrete>> typeBinding,
            GameObject gameObject,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool allowEmpty = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            GameObject? instance = null; 
            typeBinding.FromMethod(_ =>
            {
                instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                var components = instance.GetComponentsInParent<TConcrete>();
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
                typeBinding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
            return typeBinding;
        }
        
        public static TypeBinding<List<TInterface>, List<TConcrete>> FromInstantiateGameObjectGetComponentsInChildren<TInterface, TConcrete>(
            this TypeBinding<List<TInterface>, List<TConcrete>> typeBinding,
            GameObject gameObject,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool allowEmpty = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            GameObject? instance = null;
            typeBinding.FromMethod(_ =>
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
                typeBinding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
            return typeBinding;
        }
        
        public static TypeBinding<TInterface, TConcrete> FromInstantiateGameObjectAddComponent<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            GameObject gameObject,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            typeBinding.FromMethod(_ =>
            {
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                return instance.AddComponent<TConcrete>();
            });
            if (destroyOnDispose)
            {
                typeBinding.Dispose((o, c) => Object.Destroy(o.gameObject));
            }
            return typeBinding;
        }
        
        #endregion
        
        #region From Object Resource

        public static TypeBinding<TInterface, TConcrete> FromObjectResource<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            string path
        )
            where TConcrete : UnityEngine.Object
        {
            typeBinding.FromMethod(_ => Resources.Load<TConcrete>(path));
            return typeBinding;
        }

        #endregion
        
        #region From Instantiate GameObject Resource
        
        public static TypeBinding<TInterface, TConcrete> FromInstantiateGameObjectResourceGetComponent<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            string path,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            typeBinding.FromMethod(_ =>
            {
                var gameObject = Resources.Load<GameObject>(path);
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                return instance.GetComponent<TConcrete>();
            });
            if (destroyOnDispose)
            {
                typeBinding.Dispose((o, c) => Object.Destroy(o.gameObject));
            }
            return typeBinding;
        }
        
        public static TypeBinding<TInterface, TConcrete> FromInstantiateGameObjectResourceGetComponentInParent<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            string path,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            GameObject? instance = null;
            typeBinding.FromMethod(_ =>
            {
                var gameObject = Resources.Load<GameObject>(path);
                instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                return instance.GetComponentInParent<TConcrete>();
            });
            if (destroyOnDispose)
            {
                typeBinding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
            return typeBinding;
        }
        
        public static TypeBinding<TInterface, TConcrete> FromInstantiateGameObjectResourceGetComponentInChildren<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            string path,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            GameObject? instance = null;
            typeBinding.FromMethod(_ =>
            {
                var gameObject = Resources.Load<GameObject>(path);
                instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                return instance.GetComponentInChildren<TConcrete>();
            });
            if (destroyOnDispose)
            {
                typeBinding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
            return typeBinding;
        }
        
        public static TypeBinding<List<TInterface>, List<TConcrete>> FromInstantiateGameObjectResourceGetComponents<TInterface, TConcrete>(
            this TypeBinding<List<TInterface>, List<TConcrete>> typeBinding,
            string path,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool allowEmpty = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            GameObject? instance = null;
            typeBinding.FromMethod(_ =>
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
                typeBinding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
            return typeBinding;
        }
        
        public static TypeBinding<List<TInterface>, List<TConcrete>> FromInstantiateGameObjectResourceGetComponentsInChildren<TInterface, TConcrete>(
            this TypeBinding<List<TInterface>, List<TConcrete>> typeBinding,
            string path,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool allowEmpty = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            GameObject? instance = null;
            typeBinding.FromMethod(_ =>
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
                typeBinding.Dispose((o, c) =>
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                });
            }
            return typeBinding;
        }
        
        public static TypeBinding<TInterface, TConcrete> FromInstantiateGameObjectResourceAddComponent<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            string path,
            Transform? parent = null,
            bool worldPositionStays = false,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            typeBinding.FromMethod(_ =>
            {
                var gameObject = Resources.Load<GameObject>(path);
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                return instance.AddComponent<TConcrete>();
            });
            if (destroyOnDispose)
            {
                typeBinding.Dispose((o, c) => Object.Destroy(o.gameObject));
            }
            return typeBinding;
        }
        
        #endregion
        
    }
}
