using System;
using Pep.Input;
using Pep.Scoring;
using UnityEngine;
using UnityEngine.UI;

namespace Pep.Minigames.Cooking
{
    public class PanFlickMinigame : MonoBehaviour
    {
        [Header("Session")]
        [SerializeField] private float totalDuration = 8f;
        [SerializeField] private int requiredFlickCount = 1;
        [SerializeField] private float idealWindowStart = 0.25f;
        [SerializeField] private float idealWindowEnd = 0.7f;

        [Header("Runtime")]
        [SerializeField] private bool createUiOnStart = true;
        [SerializeField] private FlickDetector flickDetector;
        [SerializeField] private ScoringManager scoringManager;

        public event Action<float, bool> OnPanFlickCompleted;

        public bool IsRunning => isRunning;
        public int FlickCount => flickCount;

        private Canvas rootCanvas;
        private RectTransform panelRoot;
        private Text statusText;
        private Text timerText;
        private Text counterText;
        private Slider timerSlider;
        private bool uiBuilt;
        private bool isRunning;
        private float remainingTime;
        private float firstFlickRatio = -1f;
        private int flickCount;

        private void Awake()
        {
            if (flickDetector == null)
            {
                flickDetector = GetComponent<FlickDetector>();
            }
        }

        private void Start()
        {
            if (createUiOnStart) BuildRuntimeUi();
            if (isRunning)
                panelRoot?.gameObject.SetActive(true);
            else
                Begin();
        }

        private void Update()
        {
            if (!isRunning) return;

            remainingTime -= Time.deltaTime;
            HandleFlickInput();
            UpdateUi();

            if (flickCount >= Mathf.Max(1, requiredFlickCount) || remainingTime <= 0f)
            {
                Finish();
            }
        }

        public void Begin()
        {
            if (flickDetector != null)
            {
                flickDetector.Calibrate();
            }

            remainingTime = Mathf.Max(1f, totalDuration);
            flickCount = 0;
            firstFlickRatio = -1f;
            isRunning = true;
            if (panelRoot != null) panelRoot.gameObject.SetActive(true);
            UpdateUi();
        }

        public void Stop()
        {
            isRunning = false;
            if (panelRoot != null) panelRoot.gameObject.SetActive(false);
            UpdateUi();
        }

        public void Configure(FlickDetector detector, ScoringManager manager)
        {
            flickDetector = detector;
            scoringManager = manager;
        }

        private void HandleFlickInput()
        {
            if (flickDetector != null && flickDetector.ConsumeFlick(out _, out _))
            {
                RegisterFlick();
            }
        }

        private void RegisterFlick()
        {
            flickCount++;
            if (firstFlickRatio < 0f)
            {
                float elapsed = totalDuration - remainingTime;
                firstFlickRatio = totalDuration > 0f ? Mathf.Clamp01(elapsed / totalDuration) : 1f;
            }
        }

        private void Finish()
        {
            if (!isRunning) return;
            isRunning = false;

            float score = CalculateScore();
            bool success = score >= 55f;

            if (scoringManager != null)
            {
                scoringManager.ReportStepScore("pep/PanFlick", "Pan Flick", score);
            }

            UpdateUi();
            if (panelRoot != null) panelRoot.gameObject.SetActive(false);
            OnPanFlickCompleted?.Invoke(score, success);
        }

        private float CalculateScore()
        {
            if (flickCount <= 0) return 0f;

            float timingScore;
            if (firstFlickRatio >= idealWindowStart && firstFlickRatio <= idealWindowEnd)
            {
                timingScore = 100f;
            }
            else if (firstFlickRatio < idealWindowStart)
            {
                float t = Mathf.Clamp01(firstFlickRatio / Mathf.Max(0.001f, idealWindowStart));
                timingScore = Mathf.Lerp(35f, 90f, t);
            }
            else
            {
                float denom = Mathf.Max(0.001f, 1f - idealWindowEnd);
                float t = Mathf.Clamp01((firstFlickRatio - idealWindowEnd) / denom);
                timingScore = Mathf.Lerp(90f, 30f, t);
            }

            float countScore = Mathf.Clamp01((float)flickCount / Mathf.Max(1, requiredFlickCount));
            return Mathf.Clamp(timingScore * countScore, 0f, 100f);
        }

        public void BuildRuntimeUi()
        {
            if (uiBuilt) return;

            rootCanvas = FindObjectOfType<Canvas>();
            if (rootCanvas == null)
            {
                var canvasObject = new GameObject("PepPanFlickCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                rootCanvas = canvasObject.GetComponent<Canvas>();
                rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            var panelObject = new GameObject("PepPanFlickPanel", typeof(RectTransform), typeof(Image));
            panelRoot = panelObject.GetComponent<RectTransform>();
            panelRoot.SetParent(rootCanvas.transform, false);
            panelRoot.anchorMin = new Vector2(0.5f, 0.5f);
            panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
            panelRoot.pivot = new Vector2(0.5f, 0.5f);
            panelRoot.sizeDelta = new Vector2(740f, 320f);
            panelRoot.anchoredPosition = new Vector2(0f, 0f);
            panelObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            var titleText = CreateText("Title", panelRoot, "Pan Flick", 34, new Vector2(0f, 108f));
            titleText.alignment = TextAnchor.MiddleCenter;

            statusText = CreateText("Status", panelRoot, "Flick your phone", 22, new Vector2(0f, 62f));
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.GetComponent<RectTransform>().sizeDelta = new Vector2(620f, 52f);

            counterText = CreateText("Counter", panelRoot, "0 / 1", 28, new Vector2(0f, 16f));
            counterText.alignment = TextAnchor.MiddleCenter;

            timerText = CreateText("Timer", panelRoot, "00.0s", 26, new Vector2(0f, -30f));
            timerText.alignment = TextAnchor.MiddleCenter;

            timerSlider = CreateSlider("TimerSlider", panelRoot, new Vector2(0f, -86f), new Vector2(560f, 22f), 0f, totalDuration, totalDuration, false);

            panelRoot.gameObject.SetActive(false);
            uiBuilt = true;
        }

        private void UpdateUi()
        {
            if (!uiBuilt) return;

            if (timerSlider != null)
            {
                timerSlider.maxValue = totalDuration;
                timerSlider.value = Mathf.Clamp(remainingTime, 0f, totalDuration);
            }

            if (timerText != null)
            {
                timerText.text = $"{Mathf.Clamp(remainingTime, 0f, totalDuration):00.0}s";
            }

            if (counterText != null)
            {
                counterText.text = $"{flickCount} / {Mathf.Max(1, requiredFlickCount)}";
            }

            if (statusText != null)
            {
                statusText.text = isRunning ? "Flick once quickly\n[PC: Space or F key]" : "Completed";
            }
        }

        private Text CreateText(string name, RectTransform parent, string text, int size, Vector2 anchoredPosition)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(620f, 36f);
            rect.anchoredPosition = anchoredPosition;

            var textComp = textObject.GetComponent<Text>();
            textComp.font = GetDefaultFont();
            textComp.text = text;
            textComp.fontSize = size;
            textComp.color = Color.white;
            return textComp;
        }

        private static Font GetDefaultFont()
        {
            var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return f;
        }

        private Slider CreateSlider(
            string name,
            RectTransform parent,
            Vector2 anchoredPosition,
            Vector2 size,
            float minValue,
            float maxValue,
            float initialValue,
            bool interactable)
        {
            var root = new GameObject(name, typeof(RectTransform));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.SetParent(parent, false);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = size;
            rootRect.anchoredPosition = anchoredPosition;

            CreateImageChild(rootRect, "Background", new Color(1f, 1f, 1f, 0.25f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fillArea = CreateRectChild(rootRect, "Fill Area", new Vector2(8f, 6f), new Vector2(-8f, -6f));
            var fill = CreateImageChild(fillArea, "Fill", new Color(0.9f, 0.7f, 0.1f, 0.95f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var handleArea = CreateRectChild(rootRect, "Handle Slide Area", new Vector2(8f, 5f), new Vector2(-8f, -5f));
            var handle = CreateImageChild(handleArea, "Handle", new Color(1f, 1f, 1f, 0.95f), new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(-10f, -10f), new Vector2(20f, 20f));

            var slider = root.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = initialValue;
            slider.interactable = interactable;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private RectTransform CreateRectChild(RectTransform parent, string name, Vector2 offsetMin, Vector2 offsetMax)
        {
            var child = new GameObject(name, typeof(RectTransform));
            var rect = child.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        private GameObject CreateImageChild(
            RectTransform parent,
            string name,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = child.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            child.GetComponent<Image>().color = color;
            return child;
        }
    }
}
