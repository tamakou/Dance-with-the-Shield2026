using System;
using UnityEngine;

namespace DWS
{
    public enum ArrowHitType
    {
        None = 0,
        BlockedByShield = 1,
        HitPlayer = 2,
        Expired = 3
    }

    /// <summary>
    /// Lightweight kinematic arrow projectile.
    /// - Moves via simple ballistic integration (gravity).
    /// - Optionally checks collisions (recommended only for "Threat" arrows for Quest performance).
    /// </summary>
    public sealed class ArrowProjectile : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private TrailRenderer _trail;
        [SerializeField] private Renderer _bodyRenderer;

        [Header("Tuning")]
        [SerializeField] private float _collisionRadius = 0.05f;

        // Runtime state
        private Vector3 _velocity;
        private Vector3 _gravity;
        private float _lifeRemaining;
        private bool _collidable;
        private bool _isThreat;
        private Transform _playerHmd;
        private float _playerHitRadius;
        private LayerMask _collisionMask;
        private Action<ArrowProjectile, ArrowHitType> _onHit;

        // Buffers (avoid GC)
        private static readonly Collider[] OverlapBuffer = new Collider[16];

        private MaterialPropertyBlock _mpb;

        public bool IsThreat => _isThreat;

        private void Reset()
        {
            _trail = GetComponentInChildren<TrailRenderer>();
            _bodyRenderer = GetComponentInChildren<Renderer>();
        }

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
        }

        public void Init(
            Vector3 startPosition,
            Vector3 initialVelocity,
            Vector3 gravity,
            float lifetimeSeconds,
            float collisionRadius,
            bool isThreat,
            bool collidable,
            Transform playerHmd,
            float playerHitRadius,
            LayerMask collisionMask,
            Action<ArrowProjectile, ArrowHitType> onHit)
        {
            transform.position = startPosition;
            _velocity = initialVelocity;
            _gravity = gravity;
            _lifeRemaining = Mathf.Max(0.05f, lifetimeSeconds);
            _collisionRadius = Mathf.Max(0.001f, collisionRadius);
            _isThreat = isThreat;
            _collidable = collidable;
            _playerHmd = playerHmd;
            _playerHitRadius = Mathf.Max(0.01f, playerHitRadius);
            _collisionMask = collisionMask;
            _onHit = onHit;

            // Reset trail to avoid streaks across pooled spawns.
            if (_trail != null)
            {
                _trail.Clear();
                _trail.emitting = true;
            }

            // Color coding: threat arrows have orange trail.
            SetThreatVisual(_isThreat);
        }

        private void SetThreatVisual(bool isThreat)
        {
            if (_trail != null)
            {
                if (isThreat)
                {
                    _trail.startColor = new Color(1f, 0.55f, 0.1f, 0.9f);
                    _trail.endColor = new Color(1f, 0.55f, 0.1f, 0f);
                }
                else
                {
                    _trail.startColor = new Color(0.7f, 0.9f, 1f, 0.6f);
                    _trail.endColor = new Color(0.7f, 0.9f, 1f, 0f);
                }
            }

            if (_bodyRenderer != null)
            {
                // Avoid material instancing; use MaterialPropertyBlock instead.
                _bodyRenderer.GetPropertyBlock(_mpb);
                if (_bodyRenderer.sharedMaterial != null && _bodyRenderer.sharedMaterial.HasProperty("_Color"))
                {
                    _mpb.SetColor("_Color", isThreat ? new Color(1f, 0.55f, 0.1f, 1f) : new Color(0.9f, 0.9f, 0.95f, 1f));
                    _bodyRenderer.SetPropertyBlock(_mpb);
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            Vector3 prev = transform.position;

            // Basic ballistic integration
            _velocity += _gravity * dt;
            Vector3 next = prev + _velocity * dt;

            // Collision checks (by default only threat arrows do this)
            if (_collidable)
            {
                // 1) Shield block check (sample points along the segment).
                //    We intentionally search only for ShieldMarker colliders to avoid environment interference.
                if (CheckShieldBlock(prev, next))
                {
                    _onHit?.Invoke(this, ArrowHitType.BlockedByShield);
                    return;
                }

                // 2) Player hit check (distance from segment to HMD position).
                if (_playerHmd != null && SegmentDistanceToPoint(prev, next, _playerHmd.position) <= _playerHitRadius)
                {
                    _onHit?.Invoke(this, ArrowHitType.HitPlayer);
                    return;
                }
            }

            transform.position = next;

            // Orient towards velocity
            if (_velocity.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(_velocity.normalized, Vector3.up);
            }

            _lifeRemaining -= dt;
            if (_lifeRemaining <= 0f)
            {
                _onHit?.Invoke(this, ArrowHitType.Expired);
            }
        }

        private bool CheckShieldBlock(Vector3 prev, Vector3 next)
        {
            Vector3 delta = next - prev;
            float dist = delta.magnitude;
            if (dist <= 0.0001f)
            {
                return OverlapsShieldAtPoint(next);
            }

            // Step along the segment to avoid tunneling. Only used for collidable arrows (small count).
            const float stepMeters = 0.1f;
            int steps = Mathf.Clamp(Mathf.CeilToInt(dist / stepMeters), 1, 32);
            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector3 p = Vector3.Lerp(prev, next, t);
                if (OverlapsShieldAtPoint(p))
                {
                    return true;
                }
            }

            return false;
        }

        private bool OverlapsShieldAtPoint(Vector3 p)
        {
            int layerMask = _collisionMask.value == 0 ? ~0 : _collisionMask.value;

            int count = Physics.OverlapSphereNonAlloc(
                p,
                _collisionRadius,
                OverlapBuffer,
                layerMask,
                QueryTriggerInteraction.Ignore);

            if (count <= 0) return false;

            for (int i = 0; i < count; i++)
            {
                var c = OverlapBuffer[i];
                if (c == null) continue;

                // Shield colliders should have ShieldMarker in their parent chain.
                if (c.GetComponentInParent<ShieldMarker>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static float SegmentDistanceToPoint(Vector3 a, Vector3 b, Vector3 p)
        {
            Vector3 ab = b - a;
            float abSqr = ab.sqrMagnitude;
            if (abSqr <= 1e-6f) return Vector3.Distance(a, p);

            float t = Vector3.Dot(p - a, ab) / abSqr;
            t = Mathf.Clamp01(t);
            Vector3 closest = a + ab * t;
            return Vector3.Distance(closest, p);
        }
    }
}
