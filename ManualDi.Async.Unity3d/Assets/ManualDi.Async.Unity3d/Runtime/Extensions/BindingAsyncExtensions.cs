using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ManualDi.Async.Unity3d
{
    public static class BindingUnity3dAsyncExtensions
    {
        #region From Async Instantiate
        
        /// <summary>
        /// Use this method to Call Object.InstantiateAsync
        /// </summary>
        public static Binding<TConcrete> FromAsyncInstantiateOperation<TConcrete>(
            this Binding<TConcrete> binding,
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
        
        public static Binding<TConcrete> FromAsyncInstantiateOperationGetComponent<TConcrete>(
            this Binding<TConcrete> binding,
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
        public static Binding<TConcrete> FromLoadSceneAsyncGetComponent<TConcrete>(
            this Binding<TConcrete> binding,
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
                
                // scene not loaded, but already created at the last index
                var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                
                c.QueueAsyncDispose(async () =>
                {
                    if (!asyncOperation.isDone)
                    {
                        await WaitAsyncOperation(asyncOperation, CancellationToken.None);
                    }
                    
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
        
        public static Binding<TConcrete> FromLoadSceneAsyncGetComponentInChildren<TConcrete>(
            this Binding<TConcrete> binding,
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