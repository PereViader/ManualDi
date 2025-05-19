using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ManualDi.Async.Unity3d
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
            binding.FromMethod(c =>
            {
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                var components = instance.GetComponents<TConcrete>();
                if (components.Length == 0 && !allowEmpty)
                {
                    Object.Destroy(instance);
                    return null;
                }
                var o = components.ToList();
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
            binding.FromMethod(c =>
            {
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                var components = instance.GetComponentsInChildren<TConcrete>();
                if (components.Length == 0 && !allowEmpty)
                {
                    Object.Destroy(instance);
                    return null;
                }
                var o = components.ToList();
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
        
        #region From Resource

        public static Binding<TInterface, TConcrete> FromResourceLoad<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            string path
        )
            where TConcrete : UnityEngine.Object
        {
            binding.FromMethod(_ => Resources.Load<TConcrete>(path));
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> FromResourceLoadAsync<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            string path
        )
            where TConcrete : UnityEngine.Object
        {
            binding.FromMethodAsync(async (_, ct) => await InternalResourcesLoadAsync<TConcrete>(path, ct));
            return binding;
        }

        public static Binding<TInterface, TConcrete> FromResourceLoadAsyncGetComponent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            string path
        )
            where TConcrete : UnityEngine.Component
        {
            binding.FromMethodAsync(async (_, ct) =>
            {
                var gameObject = await InternalResourcesLoadAsync<GameObject>(path, ct);
                return gameObject.GetComponent<TConcrete>();
            });
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> FromResourceLoadAsyncGetComponentInChildren<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            string path
        )
            where TConcrete : UnityEngine.Component
        {
            binding.FromMethodAsync(async (_, ct) =>
            {
                var gameObject = await InternalResourcesLoadAsync<GameObject>(path, ct);
                return gameObject.GetComponentInChildren<TConcrete>();
            });
            return binding;
        }
        
        private static async Task<TConcrete> InternalResourcesLoadAsync<TConcrete>(string path, CancellationToken ct) where TConcrete : UnityEngine.Object
        {
            var tcs = new TaskCompletionSource<TConcrete>();
            await using var ctr = ct.Register(() => tcs.TrySetCanceled());
            
            var loadAsync = Resources.LoadAsync<TConcrete>(path);
            loadAsync.completed += asyncOperation =>
            {
                var asset = ((ResourceRequest)asyncOperation).asset;
                if (asset == null)
                {
                    tcs.TrySetException(new InvalidOperationException($"Couldn't load resource at path {path}"));
                    return;
                }

                if (asset is not TConcrete concrete)
                {
                    tcs.TrySetException(new InvalidOperationException($"Loaded resource at path {path} but was expecting {typeof(TConcrete).FullName} and it was {asset.GetType().FullName}"));
                    return;
                }

                tcs.TrySetResult(concrete);
            };
            
            return await tcs.Task;
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
                var o = instance.GetComponent<TConcrete>();
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
        
        public static Binding<TInterface, TConcrete> FromInstantiateGameObjectResourceGetComponentInChildren<TInterface, TConcrete>(
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
            binding.FromMethod(c =>
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
                var o = components.ToList();
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
            binding.FromMethod(c =>
            {
                var gameObject = Resources.Load<GameObject>(path);
                var instance = Object.Instantiate(gameObject, parent, worldPositionStays);
                var components = instance.GetComponentsInChildren<TConcrete>();
                if (components.Length == 0 && !allowEmpty)
                {
                    Object.Destroy(instance);
                    return null;
                }
                var o = components.ToList();
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
    }
}
