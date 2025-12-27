using UnityEngine;
using UnityEngine.UI;

namespace DWS
{
    /// <summary>
    /// Title menu UI:
    /// - Story Mode -> Stage Select -> load Game scene
    /// - Endless Mode -> load Endless scene
    /// 
    /// This is a simple world-space Canvas. It assumes your scene has a working UI interaction setup
    /// (e.g., Meta Interaction SDK sample scenes provide PointableCanvasModule etc).
    /// </summary>
    public sealed class MenuUI : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private Transform _follow;
        [SerializeField] private float _distance = 1.8f;
        [SerializeField] private float _height = 0.0f;

        [Header("UI")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _root;

        private GameObject _mainPanel;
        private GameObject _stagePanel;

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

            ShowMain();
        }

        private void LateUpdate()
        {
            if (_follow == null || _root == null) return;

            _root.transform.position = _follow.position + _follow.forward * _distance + Vector3.up * _height;
            _root.transform.rotation = Quaternion.LookRotation(_follow.position - _root.transform.position, Vector3.up);
        }

        private void ShowMain()
        {
            if (_mainPanel != null) _mainPanel.SetActive(true);
            if (_stagePanel != null) _stagePanel.SetActive(false);
        }

        private void ShowStageSelect()
        {
            if (_mainPanel != null) _mainPanel.SetActive(false);
            if (_stagePanel != null) _stagePanel.SetActive(true);
        }

        private void Build()
        {
            _root = new GameObject("DWS_MenuUI_Root");
            _root.transform.SetParent(transform, false);

            _canvas = _root.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.worldCamera = Camera.main;

            var scaler = _root.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;

            _root.AddComponent<GraphicRaycaster>();

            var rect = _canvas.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1000f, 700f);

            Font arial = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Background panel
            _mainPanel = CreatePanel("MainPanel", rect, new Vector2(1000f, 700f));
            _stagePanel = CreatePanel("StagePanel", rect, new Vector2(1000f, 700f));

            // Title
            CreateText("Title", _mainPanel.GetComponent<RectTransform>(), arial, 72, new Vector2(0f, 250f), new Vector2(960f, 100f), TextAnchor.MiddleCenter).text = "Dance with the Shield";

            var storyBtn = CreateButton("StoryButton", _mainPanel.GetComponent<RectTransform>(), arial, "Story Mode", new Vector2(0f, 80f));
            var endlessBtn = CreateButton("EndlessButton", _mainPanel.GetComponent<RectTransform>(), arial, "Endless Mode", new Vector2(0f, -40f));

            storyBtn.onClick.AddListener(ShowStageSelect);
            endlessBtn.onClick.AddListener(() =>
            {
                if (GameSession.Instance != null) GameSession.Instance.SelectEndless();
                SceneLoader.Load(DwsSceneNames.Endless);
            });

            // Stage select UI
            CreateText("StageTitle", _stagePanel.GetComponent<RectTransform>(), arial, 60, new Vector2(0f, 250f), new Vector2(960f, 90f), TextAnchor.MiddleCenter).text = "Select Stage";

            float startX = -250f;
            float startY = 120f;
            float dx = 250f;
            float dy = 140f;

            int stage = 1;
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int stageIndex = stage;
                    var btn = CreateButton($"Stage{stageIndex}", _stagePanel.GetComponent<RectTransform>(), arial, $"Stage {stageIndex}", new Vector2(startX + dx * col, startY - dy * row));
                    btn.onClick.AddListener(() =>
                    {
                        if (GameSession.Instance != null) GameSession.Instance.SelectStoryStage(stageIndex);
                        SceneLoader.Load(DwsSceneNames.Game);
                    });
                    stage++;
                }
            }

            var backBtn = CreateButton("BackButton", _stagePanel.GetComponent<RectTransform>(), arial, "Back", new Vector2(0f, -240f));
            backBtn.onClick.AddListener(ShowMain);

            _stagePanel.SetActive(false);
        }

        private static GameObject CreatePanel(string name, RectTransform parent, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.55f);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;

            return go;
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
            rt.sizeDelta = new Vector2(420f, 110f);
            rt.anchoredPosition = anchoredPos;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<Text>();
            text.font = font;
            text.fontSize = 46;
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
