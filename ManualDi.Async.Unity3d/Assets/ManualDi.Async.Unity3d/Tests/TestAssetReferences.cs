using UnityEditor;
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
                
                var assets = AssetDatabase.FindAssets($"t:{nameof(TestAssetReferences)}");
                _instance = AssetDatabase.LoadAssetAtPath<TestAssetReferences>(AssetDatabase.GUIDToAssetPath(assets[0]));
                return _instance;
            }
        }
    }
}