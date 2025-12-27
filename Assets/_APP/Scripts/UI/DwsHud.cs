using UnityEngine;
using UnityEngine.UI;

namespace DWS
{
    /// <summary>
    /// Simple head-locked HUD for title, timer, instructions and countdown.
    /// Auto-builds UI if not assigned.
    /// </summary>
    public sealed class DwsHud : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private Transform _follow;
        [SerializeField] private float _distance = 1.5f;
        [SerializeField] private float _height = 0.0f;

        [Header("UI Refs (auto-built if null)")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _timerText;
        [SerializeField] private Text _instructionText;
        [SerializeField] private Text _countdownText;

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

        public void SetTitle(string title)
        {
            if (_titleText != null) _titleText.text = title ?? "";
        }

        public void SetInstruction(string text)
        {
            if (_instructionText != null) _instructionText.text = text ?? "";
        }

        public void SetTimerSeconds(float seconds)
        {
            if (_timerText == null) return;

            // Format mm:ss
            if (seconds < 0f) seconds = 0f;
            int total = Mathf.FloorToInt(seconds);
            int m = total / 60;
            int s = total % 60;
            _timerText.text = $"{m:00}:{s:00}";
        }

        public void SetCountdownText(string text)
        {
            if (_countdownText != null) _countdownText.text = text ?? "";
        }

        public void SetCountdownVisible(bool visible)
        {
            if (_countdownText != null) _countdownText.gameObject.SetActive(visible);
        }

        public void SetResultVisible(bool visible)
        {
            // Currently HUD itself doesn't show results; ResultUI does.
            // This hook exists for future expansion.
        }

        private void Build()
        {
            _root = new GameObject("DWS_HUD_Root");
            _root.transform.SetParent(transform, false);

            _canvas = _root.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.worldCamera = Camera.main;

            var scaler = _root.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;

            _root.AddComponent<GraphicRaycaster>();

            var rect = _canvas.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800f, 400f);

            Font arial = Resources.GetBuiltinResource<Font>("Arial.ttf");

            _titleText = CreateText("Title", rect, arial, 42, new Vector2(0f, 150f), new Vector2(760f, 80f), TextAnchor.MiddleCenter);
            _timerText = CreateText("Timer", rect, arial, 48, new Vector2(0f, 70f), new Vector2(760f, 80f), TextAnchor.MiddleCenter);
            _instructionText = CreateText("Instruction", rect, arial, 28, new Vector2(0f, 10f), new Vector2(760f, 60f), TextAnchor.MiddleCenter);
            _countdownText = CreateText("Countdown", rect, arial, 100, new Vector2(0f, -80f), new Vector2(760f, 160f), TextAnchor.MiddleCenter);
            _countdownText.gameObject.SetActive(false);
        }

        private static Text CreateText(string name, RectTransform parent, Font font, int fontSize, Vector2 anchoredPos, Vector2 size, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.text = "";

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            return text;
        }
    }
}
