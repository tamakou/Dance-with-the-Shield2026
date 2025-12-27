using System.Collections.Generic;
using UnityEngine;

namespace DWS
{
    /// <summary>
    /// Simple pool for ArrowProjectile.
    /// If no prefab is assigned, a default arrow template is created at runtime (Primitive + TrailRenderer).
    /// </summary>
    public sealed class ArrowPool : MonoBehaviour
    {
        [Header("Pooling")]
        [SerializeField] private ArrowProjectile _arrowPrefab;
        [SerializeField] private int _prewarm = 512;
        [SerializeField] private Transform _poolRoot;

        private readonly Queue<ArrowProjectile> _pool = new Queue<ArrowProjectile>(2048);

        public ArrowProjectile Prefab => _arrowPrefab;

        private void Awake()
        {
            if (_poolRoot == null)
            {
                var root = new GameObject("ArrowPool_Root");
                root.transform.SetParent(transform, false);
                _poolRoot = root.transform;
            }

            if (_arrowPrefab == null)
            {
                _arrowPrefab = CreateDefaultArrowTemplate(_poolRoot);
            }

            Prewarm(_prewarm);
        }

        public void Prewarm(int count)
        {
            count = Mathf.Max(0, count);
            for (int i = 0; i < count; i++)
            {
                var a = CreateInstance();
                Despawn(a);
            }
        }

        public ArrowProjectile Spawn()
        {
            ArrowProjectile a = _pool.Count > 0 ? _pool.Dequeue() : CreateInstance();
            a.gameObject.SetActive(true);
            return a;
        }

        public void Despawn(ArrowProjectile a)
        {
            if (a == null) return;
            a.gameObject.SetActive(false);
            a.transform.SetParent(_poolRoot, false);
            _pool.Enqueue(a);
        }

        private ArrowProjectile CreateInstance()
        {
            var a = Instantiate(_arrowPrefab, _poolRoot);
            a.gameObject.name = "Arrow";
            a.gameObject.SetActive(false);
            return a;
        }

        private static ArrowProjectile CreateDefaultArrowTemplate(Transform parent)
        {
            // Build a simple visual arrow using primitives.
            // This is NOT a prefab asset; it's a runtime template used for Instantiate.
            var go = new GameObject("ArrowTemplate_Runtime");
            go.transform.SetParent(parent, false);
            go.SetActive(false);

            // Body: a thin capsule
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(go.transform, false);
            body.transform.localScale = new Vector3(0.05f, 0.25f, 0.05f);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // point forward-ish

            // Remove collider (we do kinematic collision checks)
            var col = body.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.Destroy(col);

            // Trail
            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.25f;
            trail.minVertexDistance = 0.05f;
            trail.widthMultiplier = 0.03f;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;

            // Best-effort unlit material for trail
            var shader = Shader.Find("Unlit/Color");
            if (shader != null)
            {
                trail.material = new Material(shader);
            }

            var arrow = go.AddComponent<ArrowProjectile>();
            // Try auto-wire
            // (ArrowProjectile uses GetComponentInChildren in Reset; but Reset isn't called here)
            // So we assign serialized refs via reflection? We'll just leave it; ArrowProjectile will search in Reset if needed.
            // It's safe: the script also works without explicit refs.
            return arrow;
        }
    }
}
