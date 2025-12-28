using System.Collections;
using UnityEngine;

namespace DWS
{
    public enum StageEndReason
    {
        Cleared = 0,
        GameOver = 1
    }

    /// <summary>
    /// Main orchestrator for Game / Endless scenes.
    /// - Waits for the player to grab the shield
    /// - Runs 3..2..1..START countdown
    /// - Spawns arrows according to StageParams
    /// - Tracks stats and shows Result UI
    /// </summary>
    public sealed class DwsGameManager : MonoBehaviour
    {
        [Header("Scene References")]
        [Tooltip("Center eye (HMD) transform. In Meta sample rigs, assign the center eye anchor/camera.")]
        [SerializeField] private Transform _playerHmd;

        [Tooltip("Shield grab detector. Assign the detector that references the Shield's Grabbable.")]
        [SerializeField] private ShieldHeldDetector _shieldHeldDetector;

        [SerializeField] private ArrowPool _arrowPool;
        [SerializeField] private ArrowSpawner _arrowSpawner;

        [Header("UI")]
        [SerializeField] private DwsHud _hud;
        [SerializeField] private ResultUI _resultUI;
        [SerializeField] private HmdWarningUI _warningUI;

        [Header("Audio")]
        [SerializeField] private MusicPlayer _musicPlayer;

        [Header("Debug Overrides")]
        [SerializeField] private bool _overrideSession = false;
        [SerializeField] private GameMode _overrideMode = GameMode.Story;
        [SerializeField, Range(1, 6)] private int _overrideStoryStage = 1;

        private StageParams _stage;
        private readonly GameStats _stats = new GameStats();

        private bool _running;
        private bool _ended;
        private float _timeRemaining;
        private float _elapsed;

        private Coroutine _countdownRoutine;

        private void Reset()
        {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            _arrowSpawner = FindAnyObjectByType<ArrowSpawner>();
            _arrowPool = FindAnyObjectByType<ArrowPool>();
            _hud = FindAnyObjectByType<DwsHud>();
            _resultUI = FindAnyObjectByType<ResultUI>();
            _shieldHeldDetector = FindAnyObjectByType<ShieldHeldDetector>();
#else
            _arrowSpawner = FindObjectOfType<ArrowSpawner>();
            _arrowPool = FindObjectOfType<ArrowPool>();
            _hud = FindObjectOfType<DwsHud>();
            _resultUI = FindObjectOfType<ResultUI>();
            _shieldHeldDetector = FindObjectOfType<ShieldHeldDetector>();
#endif
        }

        private void Start()
        {
            ResolveModeAndStage();

            _stats.Reset();
            _stats.OnPlayerHit += OnPlayerHit;

            if (_hud != null)
            {
                _hud.SetTitle(_stage.displayName);
                _hud.SetInstruction("Grab the Shield to start");
                _hud.SetCountdownVisible(false);
                _hud.SetResultVisible(false);
            }

            if (_resultUI != null)
            {
                _resultUI.Hide();
            }

            if (_shieldHeldDetector != null)
            {
                _shieldHeldDetector.FirstHeld += OnShieldFirstHeld;
            }
            else
            {
                Debug.LogWarning("[DWS] ShieldHeldDetector is not assigned. The stage will auto-start after 2 seconds.");
                Invoke(nameof(AutoStartFallback), 2f);
            }

            // Prepare spawner but don't start yet
            if (_arrowSpawner != null)
            {
                _arrowSpawner.Configure(_arrowPool, _playerHmd, _stats, _warningUI, _stage, GetDifficulty01);
            }
        }

        private void OnDestroy()
        {
            _stats.OnPlayerHit -= OnPlayerHit;

            if (_shieldHeldDetector != null)
            {
                _shieldHeldDetector.FirstHeld -= OnShieldFirstHeld;
            }
        }

        private void AutoStartFallback()
        {
            if (_ended) return;
            if (_countdownRoutine != null) StopCoroutine(_countdownRoutine);
            _countdownRoutine = StartCoroutine(CountdownThenStart());
        }

        private void ResolveModeAndStage()
        {
            GameMode mode;
            int stageIndex = 1;

            if (!_overrideSession && GameSession.Instance != null)
            {
                mode = GameSession.Instance.Mode;
                stageIndex = GameSession.Instance.StoryStageIndex;
            }
            else
            {
                mode = _overrideMode;
                stageIndex = _overrideStoryStage;
            }

            if (mode == GameMode.Endless)
            {
                _stage = StageLibrary.GetEndlessStage();
            }
            else
            {
                _stage = StageLibrary.GetStoryStage(stageIndex);
            }

            // Initialize timers
            _elapsed = 0f;
            _timeRemaining = _stage.durationSeconds;
        }

        private void OnShieldFirstHeld()
        {
            if (_ended) return;
            if (_countdownRoutine != null) return;

            _countdownRoutine = StartCoroutine(CountdownThenStart());
        }

        private IEnumerator CountdownThenStart()
        {
            if (_hud != null)
            {
                _hud.SetInstruction("");
                _hud.SetCountdownVisible(true);
            }

            yield return ShowCountdown("3", 1.0f);
            yield return ShowCountdown("2", 1.0f);
            yield return ShowCountdown("1", 1.0f);
            yield return ShowCountdown("START!", 0.6f);

            if (_hud != null) _hud.SetCountdownVisible(false);

            StartStage();
        }

        private IEnumerator ShowCountdown(string text, float seconds)
        {
            if (_hud != null) _hud.SetCountdownText(text);
            yield return new WaitForSeconds(seconds);
        }

        private void StartStage()
        {
            if (_ended) return;

            _stats.Reset();
            _elapsed = 0f;
            _timeRemaining = _stage.durationSeconds;

            _running = true;

            if (_musicPlayer != null)
            {
                _musicPlayer.Play();
            }

            if (_arrowSpawner != null)
            {
                _arrowSpawner.StartSpawning();
            }

            if (_hud != null)
            {
                _hud.SetInstruction("");
            }
        }

        private void Update()
        {
            if (!_running || _ended) return;

            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            _elapsed += dt;

            if (_stage.durationSeconds > 0f)
            {
                _timeRemaining -= dt;
                if (_hud != null) _hud.SetTimerSeconds(Mathf.Max(0f, _timeRemaining));

                if (_timeRemaining <= 0f)
                {
                    EndStage(StageEndReason.Cleared);
                }
            }
            else
            {
                // Endless: show elapsed
                if (_hud != null) _hud.SetTimerSeconds(_elapsed);
            }

            if (_stage.playerHitLimit > 0 && _stats.HitPlayer >= _stage.playerHitLimit)
            {
                EndStage(StageEndReason.GameOver);
            }
        }

        private void OnPlayerHit()
        {
            // In Endless, a hit can immediately end the stage if hit limit is reached.
            if (_stage.playerHitLimit > 0 && _stats.HitPlayer >= _stage.playerHitLimit)
            {
                EndStage(StageEndReason.GameOver);
            }
        }

        private void EndStage(StageEndReason reason)
        {
            if (_ended) return;
            _ended = true;
            _running = false;

            if (_countdownRoutine != null)
            {
                StopCoroutine(_countdownRoutine);
                _countdownRoutine = null;
            }

            if (_arrowSpawner != null) _arrowSpawner.StopSpawning();
            if (_musicPlayer != null) _musicPlayer.Stop();

            // Show results
            var grade = ComputeGrade(_stats);
            if (_resultUI != null)
            {
                _resultUI.Show(
                    stageName: _stage.displayName,
                    reason: reason,
                    totalArrows: _stats.TotalSpawned,
                    hits: _stats.HitPlayer,
                    blocked: _stats.BlockedByShield,
                    grade: grade,
                    onRetry: Retry,
                    onBackToMenu: BackToMenu);
            }

            if (_hud != null)
            {
                _hud.SetResultVisible(true);
            }
        }

        private float GetDifficulty01()
        {
            // Story: difficulty grows from 0..1 across the stage duration.
            // Endless: grows from 0..1 across difficultyRampSeconds.
            float ramp = _stage.difficultyRampSeconds > 0.01f ? _stage.difficultyRampSeconds : 60f;

            if (_stage.durationSeconds > 0.01f)
            {
                // Use elapsed/duration to allow the "threat arrow count increases over time" behavior.
                return Mathf.Clamp01(_elapsed / _stage.durationSeconds);
            }

            return Mathf.Clamp01(_elapsed / ramp);
        }

        private static string ComputeGrade(GameStats stats)
        {
            // Spec did not define grading thresholds.
            // We implement a ratio-based grade (hits / total spawned) with easily adjustable cutoffs.
            if (stats.TotalSpawned <= 0) return "S";

            float hitRate = stats.HitPlayer / (float)stats.TotalSpawned;

            if (hitRate <= 0.005f) return "S";
            if (hitRate <= 0.02f) return "A";
            if (hitRate <= 0.05f) return "B";
            if (hitRate <= 0.10f) return "C";
            return "D";
        }

        private void Retry()
        {
            SceneLoader.ReloadActiveScene();
        }

        private void BackToMenu()
        {
            SceneLoader.Load(DwsSceneNames.Menu);
        }
    }
}
