using System;
using UnityEngine;
using UnityEngine.UI;

namespace DWS
{
    /// <summary>
    /// World-space result panel with Retry / Menu buttons.
    /// Uses standard Unity UI Buttons (works with Meta Interaction SDK canvas interaction setups).
    /// </summary>
    public sealed class ResultUI : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private Transform _follow;
        [SerializeField] private float _distance = 1.6f;
        [SerializeField] private float _height = -0.1f;

        [Header("UI Refs (auto-built if null)")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _root;

        [SerializeField] private Text _title;
        [SerializeField] private Text _reason;
        [SerializeField] private Text _stats;
        [SerializeField] private Text _grade;

        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _menuButton;

        private Action _onRetry;
        private Action _onBackToMenu;

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

            Hide();
        }

        private void LateUpdate()
        {
            if (_follow == null || _root == null) return;

            _root.transform.position = _follow.position + _follow.forward * _distance + Vector3.up * _height;
            _root.transform.rotation = Quaternion.LookRotation(_follow.position - _root.transform.position, Vector3.up);
        }

        public void Show(string stageName, StageEndReason reason, int totalArrows, int hits, int blocked, string grade, Action onRetry, Action onBackToMenu)
        {
            _onRetry = onRetry;
            _onBackToMenu = onBackToMenu;

            if (_title != null) _title.text = stageName ?? "";
            if (_reason != null) _reason.text = reason == StageEndReason.Cleared ? "CLEAR" : "GAME OVER";

            if (_stats != null)
            {
                _stats.text =
                    $"Total Arrows: {totalArrows}\n" +
                    $"Hits: {hits}\n" +
                    $"Blocked: {blocked}";
            }

            if (_grade != null) _grade.text = $"Score: {grade}";

            if (_root != null) _root.SetActive(true);
        }

        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }

        private void Build()
        {
            _root = new GameObject("DWS_ResultUI_Root");
            _root.transform.SetParent(transform, false);

            _canvas = _root.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.worldCamera = Camera.main;

            var scaler = _root.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;

            _root.AddComponent<GraphicRaycaster>();

            var rect = _canvas.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900f, 520f);

            Font arial = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Background panel
            var panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(rect, false);
            var panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.6f);
            var panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.sizeDelta = new Vector2(900f, 520f);
            panelRt.anchoredPosition = Vector2.zero;

            _title = CreateText("StageTitle", panelRt, arial, 44, new Vector2(0f, 200f), new Vector2(860f, 70f), TextAnchor.MiddleCenter);
            _reason = CreateText("Reason", panelRt, arial, 52, new Vector2(0f, 140f), new Vector2(860f, 80f), TextAnchor.MiddleCenter);

            _stats = CreateText("Stats", panelRt, arial, 34, new Vector2(0f, 20f), new Vector2(860f, 220f), TextAnchor.MiddleCenter);
            _grade = CreateText("Grade", panelRt, arial, 58, new Vector2(0f, -120f), new Vector2(860f, 90f), TextAnchor.MiddleCenter);

            _retryButton = CreateButton("RetryButton", panelRt, arial, "Retry", new Vector2(-180f, -210f));
            _menuButton = CreateButton("MenuButton", panelRt, arial, "Menu", new Vector2(180f, -210f));

            _retryButton.onClick.AddListener(() => _onRetry?.Invoke());
            _menuButton.onClick.AddListener(() => _onBackToMenu?.Invoke());
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

        private static Button CreateButton(string name, RectTransform parent, Font font, string label, Vector2 anchoredPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.15f);

            var btn = go.AddComponent<Button>();

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(260f, 90f);
            rt.anchoredPosition = anchoredPos;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<Text>();
            text.font = font;
            text.fontSize = 38;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;

            var trt = textGo.GetComponent<RectTransform>();
            trt.sizeDelta = rt.sizeDelta;
            trt.anchoredPosition = Vector2.zero;

            return btn;
        }
    }
}
