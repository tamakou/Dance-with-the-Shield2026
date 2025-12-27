using System;
using UnityEngine;

namespace DWS
{
    [Serializable]
    public sealed class StageParams
    {
        [Header("Identity")]
        public string stageId = "stage_1";
        public string displayName = "Stage 1";

        [Header("Time")]
        [Tooltip("0 = Endless")]
        public float durationSeconds = 60f;

        [Tooltip("Endless用: 難易度が max まで到達するまでの秒数。Storyでは durationSeconds を入れてOK。")]
        public float difficultyRampSeconds = 60f;

        [Header("Spawn Ring")]
        public float ringRadiusMeters = 100f;
        public float spawnHeightMeters = 1.6f;
        public int firePointCount = 12;

        [Header("Directions (how many firing points are active)")]
        public int activeFirePointsStart = 4;
        public int activeFirePointsEnd = 4;

        [Header("Burst (big waves)")]
        public float burstIntervalStart = 10f;
        public float burstIntervalEnd = 10f;
        public IntRange burstArrowCountStart = new IntRange(100, 200);
        public IntRange burstArrowCountEnd = new IntRange(100, 200);
        public FloatRange burstFlightTimeStart = new FloatRange(2.8f, 3.6f);
        public FloatRange burstFlightTimeEnd = new FloatRange(2.8f, 3.6f);

        [Header("Stream (random single/low-count arrows)")]
        public FloatRange streamIntervalStart = new FloatRange(1.2f, 1.4f);
        public FloatRange streamIntervalEnd = new FloatRange(1.2f, 1.4f);
        public IntRange streamArrowCountStart = new IntRange(1, 1);
        public IntRange streamArrowCountEnd = new IntRange(1, 1);
        public FloatRange streamFlightTimeStart = new FloatRange(1.8f, 2.4f);
        public FloatRange streamFlightTimeEnd = new FloatRange(1.8f, 2.4f);

        [Header("Threat arrows (orange trail, can hit player)")]
        public IntRange threateningArrowCountStart = new IntRange(1, 2);
        public IntRange threateningArrowCountEnd = new IntRange(5, 7);

        [Header("Non-threat target dispersion (meters)")]
        public FloatRange nearMissRadius = new FloatRange(1.5f, 3.0f);

        [Header("Collision / Lifetime")]
        [Tooltip("true: オレンジ(Threat)のみ当たり判定。false: 全矢に当たり判定（Questでは重い可能性）")]
        public bool onlyThreateningCanHit = true;

        public float arrowRadiusMeters = 0.05f;
        public float playerHitRadiusMeters = 0.25f;
        public float arrowLifetimeSeconds = 8f;

        [Header("Story/Endless clear conditions")]
        [Tooltip("0=無制限（Story推奨）。Endlessは 1 以上を推奨。")]
        public int playerHitLimit = 0;

        [Header("UI")]
        public float warningSeconds = 0.8f;

        [Header("Music (optional)")]
        public float musicBpm = 120f;
        public bool beatSync = false;

        public StageRuntimeSettings Evaluate(float difficulty01)
        {
            difficulty01 = Mathf.Clamp01(difficulty01);

            var rt = new StageRuntimeSettings
            {
                activeFirePoints = Mathf.Clamp(
                    Mathf.RoundToInt(Mathf.Lerp(activeFirePointsStart, activeFirePointsEnd, difficulty01)),
                    1,
                    Mathf.Max(1, firePointCount)
                ),

                burstIntervalSeconds = Mathf.Lerp(burstIntervalStart, burstIntervalEnd, difficulty01),
                burstArrowCount = IntRange.Lerp(burstArrowCountStart, burstArrowCountEnd, difficulty01),
                burstFlightTime = FloatRange.Lerp(burstFlightTimeStart, burstFlightTimeEnd, difficulty01),

                streamInterval = FloatRange.Lerp(streamIntervalStart, streamIntervalEnd, difficulty01),
                streamArrowCount = IntRange.Lerp(streamArrowCountStart, streamArrowCountEnd, difficulty01),
                streamFlightTime = FloatRange.Lerp(streamFlightTimeStart, streamFlightTimeEnd, difficulty01),

                threateningArrowCount = IntRange.Lerp(threateningArrowCountStart, threateningArrowCountEnd, difficulty01)
            };

            // Safety
            if (rt.burstIntervalSeconds < 0.05f) rt.burstIntervalSeconds = 0.05f;
            rt.burstArrowCount.ClampMinMax();
            rt.streamArrowCount.ClampMinMax();
            rt.threateningArrowCount.ClampMinMax();

            return rt;
        }
    }

    [Serializable]
    public struct StageRuntimeSettings
    {
        public int activeFirePoints;

        public float burstIntervalSeconds;
        public IntRange burstArrowCount;
        public FloatRange burstFlightTime;

        public FloatRange streamInterval;
        public IntRange streamArrowCount;
        public FloatRange streamFlightTime;

        public IntRange threateningArrowCount;
    }
}
