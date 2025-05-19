#if USE_ADDRESSABLE

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace ManualDi.Async.Unity3d
{
    public static class BindingAddressablesUnity3dExtensions
    {
        #region From Addressables Load Asset Async
        public static Binding<TInterface, TConcrete> FromAddressablesLoadAssetAsync<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            object key
        )
            where TConcrete : UnityEngine.Object
        {
            binding.FromMethodAsync(async (c, ct) => await InternalAddressablesLoadAssetAsync<TConcrete>(key, c, ct));
            return binding;
        }

        public static Binding<TInterface, TConcrete> FromAddressablesLoadAssetAsyncGetComponent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            object key
        )
            where TConcrete : UnityEngine.Component
        {
            binding.FromMethodAsync(async (c, ct) =>
            {
                var gameObject = await InternalAddressablesLoadAssetAsync<GameObject>(key, c, ct);
                return gameObject.GetComponent<TConcrete>();
            });
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> FromAddressablesLoadAssetAsyncGetComponentInChildren<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            object key
        )
            where TConcrete : UnityEngine.Component
        {
            binding.FromMethodAsync(async (c, ct) =>
            {
                var gameObject = await InternalAddressablesLoadAssetAsync<GameObject>(key, c, ct);
                return gameObject.GetComponentInChildren<TConcrete>();
            });
            return binding;
        }
        
        private static async Task<TConcrete> InternalAddressablesLoadAssetAsync<TConcrete>(
            object key, 
            IDiContainer c, 
            CancellationToken ct
        )
            where TConcrete : UnityEngine.Object
        {
            var load = Addressables.LoadAssetAsync<TConcrete>(key);
            c.QueueDispose(() => Addressables.Release(load));
            
            var asset = await load.Task.WithCancellation(ct);
            if (load.Status == AsyncOperationStatus.Failed)
            {
                throw load.OperationException;
            }

            return asset;
        }
        #endregion
        
        #region From Addressables Load Scene Async
        public static Binding<TInterface, TConcrete> FromAddressablesLoadSceneAsyncGetComponent<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            object key
        )
            where TConcrete : UnityEngine.Object
        {
            binding.FromMethodAsync(async (c, ct) =>
            {
                var sceneInstance = await InternalAddressablesLoadSceneAsync(key, c, ct);

                foreach (var gameObject in sceneInstance.Scene.GetRootGameObjects())
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
        
        public static Binding<TInterface, TConcrete> FromAddressablesLoadSceneAsyncGetComponentInChildren<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            object key
        )
            where TConcrete : UnityEngine.Component
        {
            binding.FromMethodAsync(async (c, ct) =>
            {
                var sceneInstance = await InternalAddressablesLoadSceneAsync(key, c, ct);
                
                foreach (var gameObject in sceneInstance.Scene.GetRootGameObjects())
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
        
        private static async Task<SceneInstance> InternalAddressablesLoadSceneAsync(
            object key,
            IDiContainer c,
            CancellationToken ct
        )
        {
            var load = Addressables.LoadSceneAsync(key, LoadSceneMode.Additive);
            c.QueueAsyncDispose(async () =>
            {
                await Addressables.UnloadSceneAsync(load).Task;
            });
            
            var sceneInstance = await load.Task.WithCancellation(ct);
            if (load.Status == AsyncOperationStatus.Failed)
            {
                throw load.OperationException;
            }

            return sceneInstance;
        }
        #endregion
    }
}
#endif