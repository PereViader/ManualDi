using System;
using System.Collections;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ManualDi.Async.Unity3d.Tests.PlayMode
{
    public class TestBindingAddressablesExtensions
    {
        [UnityTest]
        public IEnumerator FromAddressablesLoadAssetAsync_Resolves_WhenNotCancelled()
        {
            yield return TestExtensions.Async(async ct =>
            {
                await using var container = await new DiContainerBindings().Install(b =>
                {
                    b.Bind<GameObject>().FromAddressablesLoadAssetAsync(TestAssetReferences.Instance.GetComponentAssetReference);
                }).Build(ct);

                var gameObject = container.Resolve<GameObject>();
                Assert.IsNotNull(gameObject);
            });
        }
        
        [UnityTest]
        public IEnumerator FromAddressablesLoadAssetAsyncGetComponent_Resolves_WhenNotCancelled()
        {
            yield return TestExtensions.Async(async ct =>
            {
                await using var container = await new DiContainerBindings().Install(b =>
                {
                    b.Bind<Image>().FromAddressablesLoadAssetAsyncGetComponent(TestAssetReferences.Instance.GetComponentAssetReference);
                }).Build(ct);

                var image = container.Resolve<Image>();
                Assert.IsNotNull(image);
            });
        }
        
        [UnityTest]
        public IEnumerator FromAddressablesLoadAssetAsyncGetComponentInChildren_Resolves_WhenNotCancelled()
        {
            yield return TestExtensions.Async(async ct =>
            {
                await using var container = await new DiContainerBindings().Install(b =>
                {
                    b.Bind<Image>().FromAddressablesLoadAssetAsyncGetComponentInChildren(TestAssetReferences.Instance.GetComponentInChildrenAssetReference);
                }).Build(ct);

                var image = container.Resolve<Image>();
                Assert.IsNotNull(image);
            });
        }
        
        [UnityTest]
        public IEnumerator FromAddressablesLoadSceneAsyncGetComponent_Resolves_WhenNotCancelled()
        {
            yield return TestExtensions.Async(async ct =>
            {
                await using var container = await new DiContainerBindings().Install(b =>
                {
                    b.Bind<Image>().FromAddressablesLoadSceneAsyncGetComponent(TestAssetReferences.Instance.SceneAssetReference);
                }).Build(ct);

                var image = container.Resolve<Image>();
                Assert.IsNotNull(image);
            });
        }
        
        [UnityTest]
        public IEnumerator FromAddressablesLoadSceneAsyncGetComponent_Cancelled_DisposesProperly()
        {
            yield return TestExtensions.Async(async ct =>
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

                    await using var container = await new DiContainerBindings().Install(b =>
                    {
                        var binding = b.Bind<Image>()
                            .FromAddressablesLoadSceneAsyncGetComponent(TestAssetReferences.Instance.SceneAssetReference);

                        //Intercept delegate and use it to cancel the token on the same frame after starting the load
                        FromAsyncDelegate fromDelegate = binding.GetFromAsyncDelegateNullable()!;
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
        
        [UnityTest]
        public IEnumerator FromAddressablesLoadSceneAsyncGetComponentInChildren_Resolves_WhenNotCancelled()
        {
            yield return TestExtensions.Async(async ct =>
            {
                await using var container = await new DiContainerBindings().Install(b =>
                {
                    b.Bind<Image>().FromAddressablesLoadSceneAsyncGetComponentInChildren(TestAssetReferences.Instance.SceneAssetReference);
                }).Build(ct);
                
                var image = container.Resolve<Image>();
                Assert.IsNotNull(image);
            });
        }
        
        [UnityTest]
        public IEnumerator FromAddressablesLoadAssetAsync_Cancelled_DisposesProperly()
        {
            yield return TestExtensions.Async(async ct =>
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

                    await using var container = await new DiContainerBindings().Install(b =>
                    {
                        var binding = b.Bind<GameObject>()
                            .FromAddressablesLoadAssetAsync(TestAssetReferences.Instance.GetComponentAssetReference);

                        //Intercept delegate and use it to cancel the token on the same frame after starting the load
                        FromAsyncDelegate fromDelegate = binding.GetFromAsyncDelegateNullable()!;
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
            });
        }
    }
}