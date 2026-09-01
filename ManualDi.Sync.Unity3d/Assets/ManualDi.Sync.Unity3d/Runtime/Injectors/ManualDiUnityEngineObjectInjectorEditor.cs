#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ManualDi.Sync.Unity3d
{
    [CustomEditor(typeof(ManualDiUnityEngineObjectInjector))]
    [CanEditMultipleObjects]
    public sealed class ManualDiUnityEngineObjectInjectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Populate Injectables"))
            {
                foreach (var t in targets)
                {
                    if (t is ManualDiUnityEngineObjectInjector injector)
                    {
                        Undo.RecordObject(injector, "Populate Injectables");
                        injector.PopulateInjectables();
                        EditorUtility.SetDirty(injector);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(injector);
                    }
                }
            }
        }
    }
}
#endif
