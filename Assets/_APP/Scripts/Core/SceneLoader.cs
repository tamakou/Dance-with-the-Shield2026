using UnityEngine;
using UnityEngine.SceneManagement;

namespace DWS
{
    public static class SceneLoader
    {
        public static void Load(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("[DWS] SceneLoader.Load called with empty sceneName.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }

        public static void ReloadActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("[DWS] ReloadActiveScene failed: active scene is not valid.");
                return;
            }

            SceneManager.LoadScene(scene.name);
        }
    }
}
