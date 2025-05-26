using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ManualDi.Async.Unity3d
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
        
        //Above this line is shared code between ManualDi.Sync and ManualDi.Async
        //Below this line is ManualDi.Async specific
        
        #region From Async Instantiate
        
        /// <summary>
        /// Use this method to Call Object.InstantiateAsync
        /// </summary>
        public static Binding<TInterface, TConcrete> FromAsyncInstantiateOperation<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            Func<AsyncInstantiateOperation<TConcrete>> asyncInstantiateOperationFunc,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            binding.FromMethodAsync(async (c, ct) =>
            {
                var operation = asyncInstantiateOperationFunc.Invoke();
                var result = await WaitAsyncInstantiateOperation<TConcrete>(operation, c, destroyOnDispose, ct);
                if (result.Length == 0)
                {
                    return null;
                }

                if (result.Length > 1)
                {
                    throw new InvalidOperationException($"Async Instantiate operation for {typeof(TConcrete).FullName} succeeded but loaded more than 1 element");
                }

                return result[0];
            });
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> FromAsyncInstantiateOperationGetComponent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            Func<AsyncInstantiateOperation<GameObject>> asyncInstantiateOperationFunc,
            bool destroyOnDispose = true
        )
            where TConcrete : Component
        {
            binding.FromMethodAsync(async (c, ct) =>
            {
                var operation = asyncInstantiateOperationFunc.Invoke();
                var result = await WaitAsyncInstantiateOperation<GameObject>(operation, c, destroyOnDispose, ct);
                if (result.Length == 0)
                {
                    return null;
                }

                if (result.Length > 1)
                {
                    throw new InvalidOperationException($"Async Instantiate operation for {typeof(TConcrete).FullName} succeeded but loaded more than 1 element");
                }

                return result[0].GetComponent<TConcrete>();
            });
            return binding;
        }
        
        private static async Task<T[]> WaitAsyncInstantiateOperation<T>(
            AsyncInstantiateOperation asyncInstantiateOperation,
            IDiContainer container,
            bool destroyResultOnDispose,
            CancellationToken ct)
            where T : Object
        {
            var tcs = new TaskCompletionSource<T[]>();
            using var ctr = ct.Register(() =>
            {
                asyncInstantiateOperation.Cancel();
                tcs.TrySetCanceled();
            });
            
            if (destroyResultOnDispose)
            {
                container.QueueDispose(() =>
                {
                    if (!asyncInstantiateOperation.isDone)
                    {
                        return;
                    }

                    foreach (var o in asyncInstantiateOperation.Result)
                    {
                        Object.Destroy(o);
                    }
                });
            }

            asyncInstantiateOperation.completed += operation =>
            {
                //Snippet below taken from AsyncInstantiateOperation<T>.Result
                Object[] result = ((AsyncInstantiateOperation)operation).Result;
                tcs.TrySetResult(UnsafeUtility.As<Object[], T[]>(ref result));
            };

            return await tcs.Task;
        }
        
        #endregion
        
        #region From Load Scene Async
        public static Binding<TInterface, TConcrete> FromLoadSceneAsyncGetComponent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            string sceneName
        )
            where TConcrete : Component
        {
            binding.FromMethodAsync(async (c, ct) =>
            {
                var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                if (asyncOperation is null)
                {
                    throw new InvalidOperationException($"Could not load scene '{sceneName}' because there was no scene to load with that name");
                }
                
                // scene not loaded, but already created
                var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                
                c.QueueAsyncDispose(async () =>
                {
                    //TODO: Validate if we can unload this in the case of a cancellation where the scene is unloading before it has finished loading
                    var operation = SceneManager.UnloadSceneAsync(scene);
                    if (operation is null)
                    {
                        return;
                    }
                    await WaitAsyncOperation(operation, CancellationToken.None);
                });

                await WaitAsyncOperation(asyncOperation, ct);

                foreach (var gameObject in scene.GetRootGameObjects())
                {
                    var component = gameObject.GetComponent<TConcrete>();
                    if (component is not null)
                    {
                        return component;
                    }
                }

                return null;
            });
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> FromLoadSceneAsyncGetComponentInChildren<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            string sceneName
        )
            where TConcrete : Component
        {
            binding.FromMethodAsync(async (c, ct) =>
            {
                var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                if (asyncOperation is null)
                {
                    throw new InvalidOperationException($"Could not load scene '{sceneName}' because there was no scene to load with that name");
                }
                
                // scene not loaded, but already created
                var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                
                c.QueueAsyncDispose(async () =>
                {
                    //TODO: Validate if we can unload this in the case of a cancellation where the scene is unloading before it has finished loading
                    var operation = SceneManager.UnloadSceneAsync(scene)!;
                    await WaitAsyncOperation(operation, CancellationToken.None);
                });

                await WaitAsyncOperation(asyncOperation, ct);

                foreach (var gameObject in scene.GetRootGameObjects())
                {
                    var component = gameObject.GetComponentInChildren<TConcrete>();
                    if (component is not null)
                    {
                        return component;
                    }
                }

                return null;
            });
            return binding;
        }
        
        private static async Task WaitAsyncOperation(AsyncOperation asyncOperation, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<bool>();
            using var ctr = ct.Register(() =>
            {
                tcs.TrySetCanceled();
            });

            asyncOperation.completed += _ =>
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }
                
                tcs.TrySetResult(true);
            };
        
            await tcs.Task;
        }
        
        #endregion
    }
}
