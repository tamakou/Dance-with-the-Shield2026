using UnityEngine;

namespace DWS
{
    /// <summary>
    /// Cross-scene session state (selected mode/stage). Lives across scene loads via DontDestroyOnLoad.
    /// </summary>
    public sealed class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        [Header("Runtime Selection (read-only in Play)")]
        [SerializeField] private GameMode _mode = GameMode.Story;
        [SerializeField, Range(1, 6)] private int _storyStageIndex = 1;

        public GameMode Mode => _mode;
        public int StoryStageIndex => _storyStageIndex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate()
        {
            // If the user already placed a GameSession in a bootstrap scene, don't duplicate it.
            if (Instance != null) return;

            var go = new GameObject("DWS_GameSession");
            Instance = go.AddComponent<GameSession>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SelectStoryStage(int stageIndex)
        {
            _mode = GameMode.Story;
            _storyStageIndex = Mathf.Clamp(stageIndex, 1, 6);
        }

        public void SelectEndless()
        {
            _mode = GameMode.Endless;
        }
    }
}
