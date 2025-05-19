using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ManualDi.Async.Unity3d.Tests.PlayMode
{
    [CreateAssetMenu(fileName = "TestAddressablesAssets", menuName = "Test/TestAddressablesAssets")]
    public class TestAddressablesAssets : ScriptableObject
    {
        public AssetReference GetComponentAsset;
        public AssetReference GetComponentInChildrenAsset;
        public AssetReference LoadSceneAsset;
    }
}