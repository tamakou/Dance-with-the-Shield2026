using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DWS
{
    /// <summary>
    /// Head-locked warning UI. Shows short-lived indicators pointing to firing directions.
    /// </summary>
    public sealed class HmdWarningUI : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private Transform _follow;
        [SerializeField] private float _distance = 1.2f;
        [SerializeField] private float _height = 0.0f;

        [Header("Layout")]
        [SerializeField] private float _ringRadius = 160f;

        [Header("UI Refs (auto-built if null)")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _container;

        private readonly List<Text> _indicators = new List<Text>(16);
        private Coroutine _hideRoutine;
        private GameObject _root;

        private void Awake()
        {
            if (_follow == null)
            {
                var cam = Camera.main;
                if (cam != null) _follow = cam.transform;
            }

            if (_canvas == null)
            {
                Build();
            }
        }

        private void LateUpdate()
        {
            if (_follow == null || _root == null) return;

            _root.transform.position = _follow.position + _follow.forward * _distance + Vector3.up * _height;
            _root.transform.rotation = Quaternion.LookRotation(_follow.position - _root.transform.position, Vector3.up);
        }

        public void ShowWarnings(IReadOnlyList<Vector3> worldDirections, float seconds)
        {
            if (_follow == null || _container == null) return;

            if (_hideRoutine != null) StopCoroutine(_hideRoutine);

            // Hide all existing indicators
            for (int i = 0; i < _indicators.Count; i++)
            {
                if (_indicators[i] != null) _indicators[i].gameObject.SetActive(false);
            }

            int needed = worldDirections == null ? 0 : worldDirections.Count;
            EnsureIndicatorCount(needed);

            for (int i = 0; i < needed; i++)
            {
                Vector3 dir = worldDirections[i];
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.0001f) continue;
                dir.Normalize();

                // Convert world direction to head-local space
                Vector3 localDir = Quaternion.Inverse(_follow.rotation) * dir;
                localDir.y = 0f;
                localDir.Normalize();

                float angle = Mathf.Atan2(localDir.x, localDir.z); // yaw
                Vector2 pos = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * _ringRadius;

                Text t = _indicators[i];
                t.gameObject.SetActive(true);
                t.rectTransform.anchoredPosition = pos;
                t.text = "!";
                t.color = new Color(1f, 0.55f, 0.1f, 1f);
            }

            _hideRoutine = StartCoroutine(HideAfter(seconds));
        }

        private IEnumerator HideAfter(float seconds)
        {
            yield return new WaitForSeconds(Mathf.Max(0.01f, seconds));
            for (int i = 0; i < _indicators.Count; i++)
            {
                if (_indicators[i] != null) _indicators[i].gameObject.SetActive(false);
            }
            _hideRoutine = null;
        }

        private void EnsureIndicatorCount(int count)
        {
            count = Mathf.Clamp(count, 0, 16);
            if (_indicators.Count >= count) return;

            Font arial = Resources.GetBuiltinResource<Font>("Arial.ttf");
            while (_indicators.Count < count)
            {
                var go = new GameObject($"Warn_{_indicators.Count}");
                go.transform.SetParent(_container, false);

                var text = go.AddComponent<Text>();
                text.font = arial;
                text.fontSize = 60;
                text.alignment = TextAnchor.MiddleCenter;
                text.text = "!";
                text.color = new Color(1f, 0.55f, 0.1f, 1f);

                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(80f, 80f);

                go.SetActive(false);
                _indicators.Add(text);
            }
        }

        private void Build()
        {
            _root = new GameObject("DWS_WarningUI_Root");
            _root.transform.SetParent(transform, false);

            _canvas = _root.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.worldCamera = Camera.main;

            var scaler = _root.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;

            _root.AddComponent<GraphicRaycaster>();

            var rtCanvas = _canvas.GetComponent<RectTransform>();
            rtCanvas.sizeDelta = new Vector2(500f, 500f);

            var containerGo = new GameObject("Container");
            containerGo.transform.SetParent(rtCanvas, false);
            _container = containerGo.AddComponent<RectTransform>();
            _container.sizeDelta = rtCanvas.sizeDelta;
            _container.anchoredPosition = Vector2.zero;
        }
    }
}
