using System.Linq;
using UnityEngine.SceneManagement;

namespace ManualDi.Unity3d.Utils
{
    public static class UnitySceneUtils
    {
        public static bool TryFindComponentInSceneRoot<T>(Scene scene, out T component)
        {
            component = scene
                .GetRootGameObjects()
                .Select(x => x.GetComponent<T>())
                .FirstOrDefault(x => x != null);

            return component != null;
        }
    }
}
