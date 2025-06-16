using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ManualDi.Async.Unity3d.Tests.PlayMode
{
    [CreateAssetMenu(fileName = "TestAssetReferences", menuName = "Test/TestAssetReferences")]
    public class TestAssetReferences : ScriptableObject
    {
        [Header("Addressables")]
        public AssetReference GetComponentAssetReference;
        public AssetReference GetComponentInChildrenAssetReference;
        public AssetReference SceneAssetReference;
        
        [Header("Async")]
        public GameObject GetComponentPrefab;
        public GameObject GetComponentInChildrenPrefab;
        public string SceneName;
        
        
        private static TestAssetReferences _instance;

        public static TestAssetReferences Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                
                _instance = Addressables.LoadAssetAsync<TestAssetReferences>("TestAssetReferences").WaitForCompletion();
                return _instance;
            }
        }
    }
}