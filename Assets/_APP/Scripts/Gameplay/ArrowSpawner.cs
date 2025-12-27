using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DWS
{
    /// <summary>
    /// Spawns arrows in two patterns:
    /// - Burst: big waves every N seconds
    /// - Stream: low-count arrows at random intervals
    /// 
    /// Spawn positions are on a ring (radius meters) centered on the player (HMD) position.
    /// </summary>
    public sealed class ArrowSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ArrowPool _arrowPool;

        [Header("Collision")]
        [Tooltip("Shieldに専用レイヤーを割り当てた場合はここで絞る（推奨）。0の場合は全レイヤー。")]
        [SerializeField] private LayerMask _collisionMask = 0;

        [Header("Debug")]
        [SerializeField] private bool _drawDebug = false;

        private StageParams _stage;
        private Transform _playerHmd;
        private GameStats _stats;
        private HmdWarningUI _warningUI;
        private Func<float> _difficulty01;

        private Coroutine _burstRoutine;
        private Coroutine _streamRoutine;

        private readonly List<ArrowProjectile> _active = new List<ArrowProjectile>(2048);

        public void Configure(ArrowPool pool, Transform playerHmd, GameStats stats, HmdWarningUI warningUI, StageParams stage, Func<float> difficulty01)
        {
            _arrowPool = pool;
            _playerHmd = playerHmd;
            _stats = stats;
            _warningUI = warningUI;
            _stage = stage;
            _difficulty01 = difficulty01;
        }

        public void StartSpawning()
        {
            if (_arrowPool == null)
            {
                Debug.LogError("[DWS] ArrowSpawner: ArrowPool is missing.");
                return;
            }
            if (_playerHmd == null)
            {
                Debug.LogError("[DWS] ArrowSpawner: PlayerHmd is missing.");
                return;
            }
            if (_stats == null)
            {
                Debug.LogError("[DWS] ArrowSpawner: Stats is missing.");
                return;
            }
            if (_stage == null)
            {
                Debug.LogError("[DWS] ArrowSpawner: Stage is missing.");
                return;
            }
            if (_difficulty01 == null)
            {
                _difficulty01 = () => 0f;
            }

            StopSpawning();

            _burstRoutine = StartCoroutine(BurstLoop());
            _streamRoutine = StartCoroutine(StreamLoop());
        }

        public void StopSpawning()
        {
            if (_burstRoutine != null) StopCoroutine(_burstRoutine);
            if (_streamRoutine != null) StopCoroutine(_streamRoutine);
            _burstRoutine = null;
            _streamRoutine = null;

            // Despawn active arrows
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var a = _active[i];
                if (a != null) _arrowPool.Despawn(a);
            }
            _active.Clear();
        }

        private IEnumerator BurstLoop()
        {
            while (true)
            {
                float d = _difficulty01();
                StageRuntimeSettings rt = _stage.Evaluate(d);

                float interval = Mathf.Max(0.05f, rt.burstIntervalSeconds);
                yield return new WaitForSeconds(interval);

                SpawnBurst(rt, d);
            }
        }

        private IEnumerator StreamLoop()
        {
            while (true)
            {
                float d = _difficulty01();
                StageRuntimeSettings rt = _stage.Evaluate(d);

                float wait = Mathf.Max(0.02f, rt.streamInterval.Random());
                yield return new WaitForSeconds(wait);

                SpawnStream(rt, d);
            }
        }

        private void SpawnBurst(StageRuntimeSettings rt, float difficulty01)
        {
            int arrowCount = rt.burstArrowCount.RandomInclusive();
            SpawnWave(rt, arrowCount, rt.burstFlightTime, difficulty01);
        }

        private void SpawnStream(StageRuntimeSettings rt, float difficulty01)
        {
            int arrowCount = rt.streamArrowCount.RandomInclusive();
            SpawnWave(rt, arrowCount, rt.streamFlightTime, difficulty01);
        }

        private void SpawnWave(StageRuntimeSettings rt, int arrowCount, FloatRange flightTimeRange, float difficulty01)
        {
            if (arrowCount <= 0) return;

            int threatCount = Mathf.Clamp(rt.threateningArrowCount.RandomInclusive(), 0, arrowCount);

            // Choose active firing point indices (evenly distributed around the ring).
            int firePoints = Mathf.Max(1, _stage.firePointCount);
            int active = Mathf.Clamp(rt.activeFirePoints, 1, firePoints);
            var activeIndices = GetEvenlyDistributedIndices(firePoints, active);

            // Gather warning directions for threat arrows (one per firing point used).
            var warningDirs = new List<Vector3>(active);

            Vector3 playerPos = _playerHmd.position;

            for (int i = 0; i < arrowCount; i++)
            {
                bool isThreat = i < threatCount;
                bool collidable = !_stage.onlyThreateningCanHit || isThreat;

                int fireIndex = activeIndices[UnityEngine.Random.Range(0, activeIndices.Count)];
                Vector3 fireDir = DirFromFireIndex(fireIndex, firePoints);

                Vector3 spawnPos = playerPos + fireDir * _stage.ringRadiusMeters + Vector3.up * _stage.spawnHeightMeters;

                Vector3 targetPos;
                if (isThreat)
                {
                    targetPos = playerPos; // Aim at the player's current HMD position (approx).
                    // Slight randomness so multiple threat arrows aren't identical
                    targetPos += new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.05f, 0.15f), UnityEngine.Random.Range(-0.1f, 0.1f));
                }
                else
                {
                    // Near-miss around the player
                    float r = _stage.nearMissRadius.Random();
                    float a = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * r;
                    targetPos = playerPos + offset;
                    targetPos += Vector3.up * UnityEngine.Random.Range(-0.2f, 0.2f);
                }

                float tFlight = Mathf.Max(0.25f, flightTimeRange.Random());
                Vector3 g = Physics.gravity;
                Vector3 v0 = (targetPos - spawnPos - 0.5f * g * tFlight * tFlight) / tFlight;

                var arrow = _arrowPool.Spawn();
                arrow.transform.SetParent(transform, false);

                _active.Add(arrow);

                _stats.RegisterSpawn(isThreat);

                arrow.Init(
                    startPosition: spawnPos,
                    initialVelocity: v0,
                    gravity: g,
                    lifetimeSeconds: _stage.arrowLifetimeSeconds,
                    collisionRadius: _stage.arrowRadiusMeters,
                    isThreat: isThreat,
                    collidable: collidable,
                    playerHmd: _playerHmd,
                    playerHitRadius: _stage.playerHitRadiusMeters,
                    collisionMask: _collisionMask,
                    onHit: OnArrowHit);

                if (isThreat && _warningUI != null)
                {
                    // Collect one warning per firing direction (avoid spamming)
                    if (!ContainsDirectionApprox(warningDirs, fireDir))
                    {
                        warningDirs.Add(fireDir);
                    }
                }

                if (_drawDebug && isThreat)
                {
                    Debug.DrawLine(spawnPos, targetPos, Color.red, 1.0f);
                }
            }

            if (_warningUI != null && warningDirs.Count > 0)
            {
                _warningUI.ShowWarnings(warningDirs, _stage.warningSeconds);
            }
        }

        private void OnArrowHit(ArrowProjectile arrow, ArrowHitType hitType)
        {
            if (arrow == null) return;

            switch (hitType)
            {
                case ArrowHitType.BlockedByShield:
                    _stats?.RegisterBlocked();
                    break;
                case ArrowHitType.HitPlayer:
                    _stats?.RegisterPlayerHit();
                    break;
                case ArrowHitType.Expired:
                default:
                    break;
            }

            _active.Remove(arrow);
            _arrowPool.Despawn(arrow);
        }

        private static List<int> GetEvenlyDistributedIndices(int totalPoints, int activePoints)
        {
            var list = new List<int>(activePoints);
            for (int i = 0; i < activePoints; i++)
            {
                int idx = Mathf.FloorToInt(i * (totalPoints / (float)activePoints));
                idx = Mathf.Clamp(idx, 0, totalPoints - 1);
                if (!list.Contains(idx)) list.Add(idx);
            }

            // In case rounding produced duplicates (rare), fill remaining randomly.
            while (list.Count < activePoints)
            {
                int idx = UnityEngine.Random.Range(0, totalPoints);
                if (!list.Contains(idx)) list.Add(idx);
            }

            return list;
        }

        private static Vector3 DirFromFireIndex(int index, int totalPoints)
        {
            float angle = index * (360f / totalPoints);
            Quaternion rot = Quaternion.Euler(0f, angle, 0f);
            return rot * Vector3.forward;
        }

        private static bool ContainsDirectionApprox(List<Vector3> dirs, Vector3 dir)
        {
            const float dotThreshold = 0.99f;
            dir.y = 0f;
            dir.Normalize();

            for (int i = 0; i < dirs.Count; i++)
            {
                Vector3 d = dirs[i];
                d.y = 0f;
                d.Normalize();
                if (Vector3.Dot(d, dir) >= dotThreshold) return true;
            }
            return false;
        }
    }
}
