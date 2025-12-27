using UnityEngine;

namespace DWS
{
    /// <summary>
    /// Code-based default stage parameters.
    /// If you prefer ScriptableObjects, you can extend this later, but this gives an out-of-box playable baseline.
    /// </summary>
    public static class StageLibrary
    {
        private static StageParams[] _story;
        private static StageParams _endless;

        public static StageParams GetStoryStage(int stageIndex)
        {
            EnsureInit();
            stageIndex = Mathf.Clamp(stageIndex, 1, 6);
            // Return a copy to prevent accidental runtime mutation of the cached objects.
            return Clone(_story[stageIndex - 1]);
        }

        public static StageParams GetEndlessStage()
        {
            EnsureInit();
            return Clone(_endless);
        }

        private static void EnsureInit()
        {
            if (_story != null && _story.Length == 6 && _endless != null) return;

            _story = new StageParams[6];

            // Design goals derived from the user's spec:
            // - Stage1..6: 1..6 minutes
            // - Arrow waves (burst) roughly around 10s interval, varies by stage.
            // - Stream arrows random interval ~0.7..1.4s, varies by stage.
            // - Burst arrow count 100..1000, stage-dependent.
            // - Threat arrows 1..20, increasing over time (we set Start->End to ramp with stage progress).
            //
            // NOTE: Only the Threat arrows are collidable by default (performance).
            _story[0] = new StageParams
            {
                stageId = "story_1",
                displayName = "Stage 1",
                durationSeconds = 60f,
                difficultyRampSeconds = 60f,

                ringRadiusMeters = 100f,
                spawnHeightMeters = 1.6f,
                firePointCount = 12,

                activeFirePointsStart = 4,
                activeFirePointsEnd = 4,

                burstIntervalStart = 10f,
                burstIntervalEnd = 10f,
                burstArrowCountStart = new IntRange(100, 200),
                burstArrowCountEnd = new IntRange(100, 200),
                burstFlightTimeStart = new FloatRange(2.8f, 3.6f),
                burstFlightTimeEnd = new FloatRange(2.8f, 3.6f),

                streamIntervalStart = new FloatRange(1.2f, 1.4f),
                streamIntervalEnd = new FloatRange(1.2f, 1.4f),
                streamArrowCountStart = new IntRange(1, 1),
                streamArrowCountEnd = new IntRange(1, 1),
                streamFlightTimeStart = new FloatRange(1.8f, 2.4f),
                streamFlightTimeEnd = new FloatRange(1.8f, 2.4f),

                threateningArrowCountStart = new IntRange(1, 2),
                threateningArrowCountEnd = new IntRange(3, 5),

                nearMissRadius = new FloatRange(2.0f, 4.0f),

                onlyThreateningCanHit = true,
                arrowRadiusMeters = 0.05f,
                playerHitRadiusMeters = 0.25f,
                arrowLifetimeSeconds = 10f,

                playerHitLimit = 0,
                warningSeconds = 0.8f,

                musicBpm = 120f,
                beatSync = false
            };

            _story[1] = new StageParams
            {
                stageId = "story_2",
                displayName = "Stage 2",
                durationSeconds = 120f,
                difficultyRampSeconds = 120f,

                ringRadiusMeters = 100f,
                spawnHeightMeters = 1.6f,
                firePointCount = 12,

                activeFirePointsStart = 6,
                activeFirePointsEnd = 6,

                burstIntervalStart = 9f,
                burstIntervalEnd = 9f,
                burstArrowCountStart = new IntRange(150, 300),
                burstArrowCountEnd = new IntRange(150, 300),
                burstFlightTimeStart = new FloatRange(2.6f, 3.4f),
                burstFlightTimeEnd = new FloatRange(2.6f, 3.4f),

                streamIntervalStart = new FloatRange(1.1f, 1.3f),
                streamIntervalEnd = new FloatRange(1.1f, 1.3f),
                streamArrowCountStart = new IntRange(1, 1),
                streamArrowCountEnd = new IntRange(1, 1),
                streamFlightTimeStart = new FloatRange(1.6f, 2.2f),
                streamFlightTimeEnd = new FloatRange(1.6f, 2.2f),

                threateningArrowCountStart = new IntRange(1, 3),
                threateningArrowCountEnd = new IntRange(5, 7),

                nearMissRadius = new FloatRange(1.8f, 3.8f),

                onlyThreateningCanHit = true,
                arrowRadiusMeters = 0.05f,
                playerHitRadiusMeters = 0.25f,
                arrowLifetimeSeconds = 10f,

                playerHitLimit = 0,
                warningSeconds = 0.8f,

                musicBpm = 124f,
                beatSync = false
            };

            _story[2] = new StageParams
            {
                stageId = "story_3",
                displayName = "Stage 3",
                durationSeconds = 180f,
                difficultyRampSeconds = 180f,

                ringRadiusMeters = 100f,
                spawnHeightMeters = 1.6f,
                firePointCount = 12,

                activeFirePointsStart = 8,
                activeFirePointsEnd = 8,

                burstIntervalStart = 8f,
                burstIntervalEnd = 8f,
                burstArrowCountStart = new IntRange(250, 450),
                burstArrowCountEnd = new IntRange(250, 450),
                burstFlightTimeStart = new FloatRange(2.4f, 3.2f),
                burstFlightTimeEnd = new FloatRange(2.4f, 3.2f),

                streamIntervalStart = new FloatRange(1.0f, 1.2f),
                streamIntervalEnd = new FloatRange(1.0f, 1.2f),
                streamArrowCountStart = new IntRange(1, 1),
                streamArrowCountEnd = new IntRange(1, 1),
                streamFlightTimeStart = new FloatRange(1.5f, 2.0f),
                streamFlightTimeEnd = new FloatRange(1.5f, 2.0f),

                threateningArrowCountStart = new IntRange(2, 4),
                threateningArrowCountEnd = new IntRange(8, 10),

                nearMissRadius = new FloatRange(1.6f, 3.6f),

                onlyThreateningCanHit = true,
                arrowRadiusMeters = 0.05f,
                playerHitRadiusMeters = 0.25f,
                arrowLifetimeSeconds = 9f,

                playerHitLimit = 0,
                warningSeconds = 0.85f,

                musicBpm = 128f,
                beatSync = false
            };

            _story[3] = new StageParams
            {
                stageId = "story_4",
                displayName = "Stage 4",
                durationSeconds = 240f,
                difficultyRampSeconds = 240f,

                ringRadiusMeters = 100f,
                spawnHeightMeters = 1.6f,
                firePointCount = 12,

                activeFirePointsStart = 10,
                activeFirePointsEnd = 10,

                burstIntervalStart = 7f,
                burstIntervalEnd = 7f,
                burstArrowCountStart = new IntRange(400, 600),
                burstArrowCountEnd = new IntRange(400, 600),
                burstFlightTimeStart = new FloatRange(2.2f, 3.0f),
                burstFlightTimeEnd = new FloatRange(2.2f, 3.0f),

                streamIntervalStart = new FloatRange(0.9f, 1.1f),
                streamIntervalEnd = new FloatRange(0.9f, 1.1f),
                streamArrowCountStart = new IntRange(1, 1),
                streamArrowCountEnd = new IntRange(1, 1),
                streamFlightTimeStart = new FloatRange(1.4f, 1.9f),
                streamFlightTimeEnd = new FloatRange(1.4f, 1.9f),

                threateningArrowCountStart = new IntRange(3, 6),
                threateningArrowCountEnd = new IntRange(10, 14),

                nearMissRadius = new FloatRange(1.5f, 3.4f),

                onlyThreateningCanHit = true,
                arrowRadiusMeters = 0.05f,
                playerHitRadiusMeters = 0.25f,
                arrowLifetimeSeconds = 9f,

                playerHitLimit = 0,
                warningSeconds = 0.9f,

                musicBpm = 132f,
                beatSync = false
            };

            _story[4] = new StageParams
            {
                stageId = "story_5",
                displayName = "Stage 5",
                durationSeconds = 300f,
                difficultyRampSeconds = 300f,

                ringRadiusMeters = 100f,
                spawnHeightMeters = 1.6f,
                firePointCount = 12,

                activeFirePointsStart = 12,
                activeFirePointsEnd = 12,

                burstIntervalStart = 6.5f,
                burstIntervalEnd = 6.5f,
                burstArrowCountStart = new IntRange(600, 800),
                burstArrowCountEnd = new IntRange(600, 800),
                burstFlightTimeStart = new FloatRange(2.0f, 2.8f),
                burstFlightTimeEnd = new FloatRange(2.0f, 2.8f),

                streamIntervalStart = new FloatRange(0.8f, 1.0f),
                streamIntervalEnd = new FloatRange(0.8f, 1.0f),
                streamArrowCountStart = new IntRange(1, 1),
                streamArrowCountEnd = new IntRange(1, 1),
                streamFlightTimeStart = new FloatRange(1.3f, 1.8f),
                streamFlightTimeEnd = new FloatRange(1.3f, 1.8f),

                threateningArrowCountStart = new IntRange(4, 8),
                threateningArrowCountEnd = new IntRange(14, 18),

                nearMissRadius = new FloatRange(1.4f, 3.2f),

                onlyThreateningCanHit = true,
                arrowRadiusMeters = 0.05f,
                playerHitRadiusMeters = 0.25f,
                arrowLifetimeSeconds = 8.5f,

                playerHitLimit = 0,
                warningSeconds = 0.95f,

                musicBpm = 136f,
                beatSync = false
            };

            _story[5] = new StageParams
            {
                stageId = "story_6",
                displayName = "Stage 6",
                durationSeconds = 360f,
                difficultyRampSeconds = 360f,

                ringRadiusMeters = 100f,
                spawnHeightMeters = 1.6f,
                firePointCount = 12,

                activeFirePointsStart = 12,
                activeFirePointsEnd = 12,

                burstIntervalStart = 6f,
                burstIntervalEnd = 6f,
                burstArrowCountStart = new IntRange(800, 1000),
                burstArrowCountEnd = new IntRange(800, 1000),
                burstFlightTimeStart = new FloatRange(1.8f, 2.6f),
                burstFlightTimeEnd = new FloatRange(1.8f, 2.6f),

                streamIntervalStart = new FloatRange(0.7f, 0.9f),
                streamIntervalEnd = new FloatRange(0.7f, 0.9f),
                streamArrowCountStart = new IntRange(1, 1),
                streamArrowCountEnd = new IntRange(1, 1),
                streamFlightTimeStart = new FloatRange(1.2f, 1.7f),
                streamFlightTimeEnd = new FloatRange(1.2f, 1.7f),

                threateningArrowCountStart = new IntRange(5, 10),
                threateningArrowCountEnd = new IntRange(18, 20),

                nearMissRadius = new FloatRange(1.3f, 3.0f),

                onlyThreateningCanHit = true,
                arrowRadiusMeters = 0.05f,
                playerHitRadiusMeters = 0.25f,
                arrowLifetimeSeconds = 8f,

                playerHitLimit = 0,
                warningSeconds = 1.0f,

                musicBpm = 140f,
                beatSync = false
            };

            // Endless: start around Stage2-3 level, ramp to Stage6-ish over 5 minutes.
            _endless = new StageParams
            {
                stageId = "endless",
                displayName = "Endless",
                durationSeconds = 0f, // endless
                difficultyRampSeconds = 300f,

                ringRadiusMeters = 100f,
                spawnHeightMeters = 1.6f,
                firePointCount = 12,

                activeFirePointsStart = 6,
                activeFirePointsEnd = 12,

                burstIntervalStart = 9f,
                burstIntervalEnd = 6f,
                burstArrowCountStart = new IntRange(200, 350),
                burstArrowCountEnd = new IntRange(800, 1000),
                burstFlightTimeStart = new FloatRange(2.6f, 3.4f),
                burstFlightTimeEnd = new FloatRange(1.8f, 2.6f),

                streamIntervalStart = new FloatRange(1.1f, 1.3f),
                streamIntervalEnd = new FloatRange(0.7f, 0.9f),
                streamArrowCountStart = new IntRange(1, 1),
                streamArrowCountEnd = new IntRange(1, 1),
                streamFlightTimeStart = new FloatRange(1.6f, 2.2f),
                streamFlightTimeEnd = new FloatRange(1.2f, 1.7f),

                threateningArrowCountStart = new IntRange(2, 4),
                threateningArrowCountEnd = new IntRange(18, 20),

                nearMissRadius = new FloatRange(1.5f, 3.5f),

                onlyThreateningCanHit = true,
                arrowRadiusMeters = 0.05f,
                playerHitRadiusMeters = 0.25f,
                arrowLifetimeSeconds = 10f,

                playerHitLimit = 1, // 1 hit = game over (changeable)
                warningSeconds = 0.9f,

                musicBpm = 128f,
                beatSync = false
            };
        }

        private static StageParams Clone(StageParams src)
        {
            // Shallow memberwise clone is fine because StageParams only contains value types + strings.
            // (Ranges are structs.)
            return new StageParams
            {
                stageId = src.stageId,
                displayName = src.displayName,
                durationSeconds = src.durationSeconds,
                difficultyRampSeconds = src.difficultyRampSeconds,

                ringRadiusMeters = src.ringRadiusMeters,
                spawnHeightMeters = src.spawnHeightMeters,
                firePointCount = src.firePointCount,

                activeFirePointsStart = src.activeFirePointsStart,
                activeFirePointsEnd = src.activeFirePointsEnd,

                burstIntervalStart = src.burstIntervalStart,
                burstIntervalEnd = src.burstIntervalEnd,
                burstArrowCountStart = src.burstArrowCountStart,
                burstArrowCountEnd = src.burstArrowCountEnd,
                burstFlightTimeStart = src.burstFlightTimeStart,
                burstFlightTimeEnd = src.burstFlightTimeEnd,

                streamIntervalStart = src.streamIntervalStart,
                streamIntervalEnd = src.streamIntervalEnd,
                streamArrowCountStart = src.streamArrowCountStart,
                streamArrowCountEnd = src.streamArrowCountEnd,
                streamFlightTimeStart = src.streamFlightTimeStart,
                streamFlightTimeEnd = src.streamFlightTimeEnd,

                threateningArrowCountStart = src.threateningArrowCountStart,
                threateningArrowCountEnd = src.threateningArrowCountEnd,

                nearMissRadius = src.nearMissRadius,

                onlyThreateningCanHit = src.onlyThreateningCanHit,
                arrowRadiusMeters = src.arrowRadiusMeters,
                playerHitRadiusMeters = src.playerHitRadiusMeters,
                arrowLifetimeSeconds = src.arrowLifetimeSeconds,

                playerHitLimit = src.playerHitLimit,
                warningSeconds = src.warningSeconds,

                musicBpm = src.musicBpm,
                beatSync = src.beatSync
            };
        }
    }
}
