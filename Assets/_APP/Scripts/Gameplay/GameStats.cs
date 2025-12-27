using System;

namespace DWS
{
    public sealed class GameStats
    {
        public int TotalSpawned { get; private set; }
        public int ThreatSpawned { get; private set; }
        public int BlockedByShield { get; private set; }
        public int HitPlayer { get; private set; }

        public event Action OnPlayerHit;

        public void Reset()
        {
            TotalSpawned = 0;
            ThreatSpawned = 0;
            BlockedByShield = 0;
            HitPlayer = 0;
        }

        public void RegisterSpawn(bool isThreat)
        {
            TotalSpawned++;
            if (isThreat) ThreatSpawned++;
        }

        public void RegisterBlocked()
        {
            BlockedByShield++;
        }

        public void RegisterPlayerHit()
        {
            HitPlayer++;
            OnPlayerHit?.Invoke();
        }
    }
}
