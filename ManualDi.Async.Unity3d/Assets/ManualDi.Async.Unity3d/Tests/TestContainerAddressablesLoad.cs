using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ManualDi.Async.Unity3d.Tests.PlayMode
{
    public class TestContainerAddressablesLoad
    {
        private TestAddressablesAssets _config;

        [OneTimeSetUp]
        public void LoadTestAssetsConfig()
        {
            var assets = AssetDatabase.FindAssets("t:TestAddressablesAssets");
            _config = AssetDatabase.LoadAssetAtPath<TestAddressablesAssets>(AssetDatabase.GUIDToAssetPath(assets[0]));
        }

        [UnityTest]
        public IEnumerator TestLoadAddressableAsset()
        {
            yield return TestExtensions.Async(async ct =>
            {
                await using var container = await new DiContainerBindings().Install(b =>
                {
                    b.Bind<GameObject>().FromAddressablesLoadAssetAsync(_config.GetComponentAsset);
                }).Build(ct);

                var gameObject = container.Resolve<GameObject>();
                Assert.IsNotNull(gameObject);
            });
        }
        
        [UnityTest]
        public IEnumerator TestLoadAddressableAssetGetComponent()
        {
            yield return TestExtensions.Async(async ct =>
            {
                await using var container = await new DiContainerBindings().Install(b =>
                {
                    b.Bind<Image>().FromAddressablesLoadAssetAsyncGetComponent(_config.GetComponentAsset);
                }).Build(ct);

                var image = container.Resolve<Image>();
                Assert.IsNotNull(image);
            });
        }
        
        [UnityTest]
        public IEnumerator TestLoadAddressableAssetGetComponentInChildren()
        {
            yield return TestExtensions.Async(async ct =>
            {
                await using var container = await new DiContainerBindings().Install(b =>
                {
                    b.Bind<Image>().FromAddressablesLoadAssetAsyncGetComponentInChildren(_config.GetComponentInChildrenAsset);
                }).Build(ct);

                var image = container.Resolve<Image>();
                Assert.IsNotNull(image);
            });
        }
        
        [UnityTest]
        public IEnumerator TestLoadAddressableSceneGetComponent()
        {
            yield return TestExtensions.Async(async ct =>
            {
                await using var container = await new DiContainerBindings().Install(b =>
                {
                    b.Bind<Image>().FromAddressablesLoadSceneAsyncGetComponent(_config.LoadSceneAsset);
                }).Build(ct);

                var image = container.Resolve<Image>();
                Assert.IsNotNull(image);
            });
        }
        
        [UnityTest]
        public IEnumerator TestLoadAddressableSceneGetComponentInChildren()
        {
            yield return TestExtensions.Async(async ct =>
            {
                await using var container = await new DiContainerBindings().Install(b =>
                {
                    b.Bind<Image>().FromAddressablesLoadSceneAsyncGetComponentInChildren(_config.LoadSceneAsset);
                }).Build(ct);
                
                var image = container.Resolve<Image>();
                Assert.IsNotNull(image);
            });
        }
    }
}