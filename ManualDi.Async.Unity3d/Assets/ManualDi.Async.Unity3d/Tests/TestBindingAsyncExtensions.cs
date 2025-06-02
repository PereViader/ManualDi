using System;
using System.Collections;
using System.Threading;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ManualDi.Async.Unity3d.Tests.PlayMode
{
    public class TestBindingAsyncExtensions
    {
        [UnityTest]
        public IEnumerator FromAsyncInstantiateOperationGetComponent_Resolves_WhenNotCancelled()
        {
            yield return TestExtensions.Async(async ct =>
            {
                await using var container = await new DiContainerBindings().Install(b =>
                {
                    b.Bind<Image>().FromAsyncInstantiateOperationGetComponent(() => Object.InstantiateAsync(TestAssetReferences.Instance.GetComponentPrefab));
                }).Build(ct);

                var image = container.Resolve<Image>();
                Assert.IsNotNull(image);
            });
        }
        
        [UnityTest]
        public IEnumerator FromAsyncInstantiateOperationGetComponent_Cancelled_DisposesProperly()
        {
            yield return TestExtensions.Async(async ct =>
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

                    await using var container = await new DiContainerBindings().Install(b =>
                    {
                        var binding = b.Bind<Image>()
                            .FromAsyncInstantiateOperationGetComponent(() => Object.InstantiateAsync(TestAssetReferences.Instance.GetComponentPrefab));

                        //Intercept delegate and use it to cancel the token on the same frame after starting the load
                        FromAsyncDelegate fromDelegate = binding.TryGetFromAsyncDelegate()!;
                        binding.FromMethodAsync((c, ct) =>
                        {
                            var task = fromDelegate(c, ct);
                            cts.Cancel();
                            return task;
                        });
                        
                    }).Build(cts.Token);
                    
                    Assert.Fail("Because it is cancelled during loading, this should never happen"); 
                }
                catch (OperationCanceledException)
                {
                }
                
                var gameObject = Object.FindObjectOfType<Image>();
                Assert.IsNull(gameObject);
            });
        }
        
        [UnityTest]
        public IEnumerator FromLoadSceneAsyncGetComponent_Resolves_WhenNotCancelled()
        {
            yield return TestExtensions.Async(async ct =>
            {
                await using var container = await new DiContainerBindings().Install(b =>
                {
                    b.Bind<Image>().FromLoadSceneAsyncGetComponent(TestAssetReferences.Instance.SceneName);
                }).Build(ct);

                var image = container.Resolve<Image>();
                Assert.IsNotNull(image);
            });
        }
        
        [UnityTest]
        public IEnumerator FromLoadSceneAsyncGetComponent_Cancelled_DisposesProperly()
        {
            yield return TestExtensions.Async(async ct =>
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

                    await using var container = await new DiContainerBindings().Install(b =>
                    {
                        var binding = b.Bind<Image>()
                            .FromLoadSceneAsyncGetComponent(TestAssetReferences.Instance.SceneName);

                        //Intercept delegate and use it to cancel the token on the same frame after starting the load
                        FromAsyncDelegate fromDelegate = binding.TryGetFromAsyncDelegate()!;
                        binding.FromMethodAsync((c, ct) =>
                        {
                            var task = fromDelegate(c, ct);
                            cts.Cancel();
                            return task;
                        });
                        
                    }).Build(cts.Token);
                    
                    Assert.Fail("Because it is cancelled during loading, this should never happen"); 
                }
                catch (OperationCanceledException)
                {
                }

                Assert.That(SceneManager.sceneCount, Is.EqualTo(1));
            });
        }
    }
}