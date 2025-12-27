#if UNITY_EDITOR
using System.Linq;
using Oculus.Interaction;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DWS.Editor
{
    public static class DwsSetupWizard
    {
        [MenuItem("DWS/Setup/Add DWS Managers (Menu)")]
        public static void AddMenuManagers()
        {
            EnsureEventSystem();

            var root = GameObject.Find("DWS_Menu") ?? new GameObject("DWS_Menu");
            if (root.GetComponent<MenuUI>() == null) root.AddComponent<MenuUI>();

            Selection.activeGameObject = root;
            Debug.Log("[DWS] Added MenuUI. Ensure your scene has a working Meta Interaction SDK canvas interaction setup.");
        }

        [MenuItem("DWS/Setup/Add DWS Managers (Gameplay)")]
        public static void AddGameplayManagers()
        {
            EnsureEventSystem();

            var root = GameObject.Find("DWS_Gameplay") ?? new GameObject("DWS_Gameplay");

            // Core components
            var pool = root.GetComponent<ArrowPool>() ?? root.AddComponent<ArrowPool>();
            var spawner = root.GetComponent<ArrowSpawner>() ?? root.AddComponent<ArrowSpawner>();
            var hud = root.GetComponent<DwsHud>() ?? root.AddComponent<DwsHud>();
            var warning = root.GetComponent<HmdWarningUI>() ?? root.AddComponent<HmdWarningUI>();
            var result = root.GetComponent<ResultUI>() ?? root.AddComponent<ResultUI>();
            var music = root.GetComponent<MusicPlayer>() ?? root.AddComponent<MusicPlayer>();

            var gm = root.GetComponent<DwsGameManager>() ?? root.AddComponent<DwsGameManager>();

            // Best-effort auto-wire
            var cam = Camera.main;
            if (cam == null)
            {
                var anyCam = Object.FindObjectsOfType<Camera>().FirstOrDefault();
                cam = anyCam;
            }

            if (cam != null)
            {
                SerializedObject so = new SerializedObject(gm);
                so.FindProperty("_playerHmd").objectReferenceValue = cam.transform;
                so.FindProperty("_arrowPool").objectReferenceValue = pool;
                so.FindProperty("_arrowSpawner").objectReferenceValue = spawner;
                so.FindProperty("_hud").objectReferenceValue = hud;
                so.FindProperty("_resultUI").objectReferenceValue = result;
                so.FindProperty("_warningUI").objectReferenceValue = warning;
                so.FindProperty("_musicPlayer").objectReferenceValue = music;

                // ShieldHeldDetector: create one if missing
                var det = Object.FindObjectOfType<ShieldHeldDetector>();
                if (det == null)
                {
                    var detGo = new GameObject("DWS_ShieldHeldDetector");
                    det = detGo.AddComponent<ShieldHeldDetector>();

                    // Try find a Grabbable under ShieldMarker
                    var grabbable = Object.FindObjectsOfType<Grabbable>()
                        .FirstOrDefault(g => g != null && g.GetComponentInParent<ShieldMarker>() != null);
                    if (grabbable != null)
                    {
                        SerializedObject detSo = new SerializedObject(det);
                        detSo.FindProperty("_grabbable").objectReferenceValue = grabbable;
                        detSo.ApplyModifiedPropertiesWithoutUndo();
                    }
                }

                so.FindProperty("_shieldHeldDetector").objectReferenceValue = det;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            Selection.activeGameObject = root;
            Debug.Log("[DWS] Added Gameplay managers. Assign ShieldMarker + Grabbable to ShieldHeldDetector, and add ShieldMarker to your shield root.");
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;

            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
            Debug.Log("[DWS] Created EventSystem (StandaloneInputModule). If you use Meta Interaction SDK's canvas input module, keep the sample's setup instead.");
        }
    }
}
#endif
