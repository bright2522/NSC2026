using System;
using Pep.Scoring;
using UnityEngine;
using UnityEngine.UI;
using UnityInput = UnityEngine.Input;

namespace Pep.Minigames.Chopping
{
    public class ChoppingMockState : MonoBehaviour
    {
        [SerializeField] private float totalDuration = 10f;
        [SerializeField] private int requiredHits = 10;
        [SerializeField] private bool createUiOnStart = true;
        [SerializeField] private bool autoStartOnEnable = false;
        [SerializeField] private ScoringManager scoringManager;

        public event Action<float, bool> OnChoppingCompleted;

        public bool IsRunning { get; private set; }
        public int HitCount => hitCount;

        private Canvas rootCanvas;
        private RectTransform panelRoot;
        private Slider progressSlider;
        private Slider timerSlider;
        private Text titleText;
        private Text statusText;
        private bool uiBuilt;

        private float remainingTime;
        private int hitCount;

        private void OnEnable()
        {
            if (autoStartOnEnable)
            {
                Begin();
            }
        }

        private void Update()
        {
            if (!IsRunning) return;

            remainingTime -= Time.deltaTime;

            if (UnityInput.GetMouseButtonDown(0) || UnityInput.GetKeyDown(KeyCode.Space))
            {
                hitCount++;
            }

            UpdateUi();

            if (hitCount >= Mathf.Max(1, requiredHits) || remainingTime <= 0f)
            {
                Finish();
            }
        }

        public void Configure(ScoringManager manager)
        {
            scoringManager = manager;
        }

        public void Begin()
        {
            if (createUiOnStart && !uiBuilt)
            {
                BuildRuntimeUi();
            }

            remainingTime = Mathf.Max(1f, totalDuration);
            hitCount = 0;
            IsRunning = true;
            if (panelRoot != null) panelRoot.gameObject.SetActive(true);
            UpdateUi();
        }

        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;
            if (panelRoot != null) panelRoot.gameObject.SetActive(false);
        }

        public void ForceComplete(float score = 70f)
        {
            if (!IsRunning) return;
            Complete(score);
        }

        private void Finish()
        {
            float ratio = Mathf.Clamp01((float)hitCount / Mathf.Max(1, requiredHits));
            float timeRatio = totalDuration <= 0f ? 1f : Mathf.Clamp01(remainingTime / totalDuration);
            float score = Mathf.Clamp(ratio * 85f + timeRatio * 15f, 0f, 100f);
            Complete(score);
        }

        private void Complete(float score)
        {
            IsRunning = false;
            bool success = score >= 50f;

            if (scoringManager != null)
            {
                scoringManager.ReportStepScore("pep/ChoppingMock", "Chopping", score);
            }

            UpdateUi();
            if (panelRoot != null) panelRoot.gameObject.SetActive(false);
            OnChoppingCompleted?.Invoke(score, success);
        }

        public void BuildRuntimeUi()
        {
            if (uiBuilt) return;

            rootCanvas = FindObjectOfType<Canvas>();
            if (rootCanvas == null)
            {
                var canvasObject = new GameObject("PepChoppingCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                rootCanvas = canvasObject.GetComponent<Canvas>();
                rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            var panelObject = new GameObject("PepChoppingPanel", typeof(RectTransform), typeof(Image));
            panelRoot = panelObject.GetComponent<RectTransform>();
            panelRoot.SetParent(rootCanvas.transform, false);
            panelRoot.anchorMin = new Vector2(1f, 1f);
            panelRoot.anchorMax = new Vector2(1f, 1f);
            panelRoot.pivot = new Vector2(1f, 1f);
            panelRoot.sizeDelta = new Vector2(520f, 200f);
            panelRoot.anchoredPosition = new Vector2(-24f, -24f);
            panelObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            titleText = CreateText("Title", panelRoot, "Chopping", 30, new Vector2(-260f, -34f));
            titleText.alignment = TextAnchor.MiddleCenter;

            statusText = CreateText("Status", panelRoot, "Click/Space to chop", 22, new Vector2(-260f, -74f));
            statusText.alignment = TextAnchor.MiddleCenter;

            progressSlider = CreateSlider("Progress", panelRoot, new Vector2(-260f, -122f), new Vector2(430f, 24f), 0f, requiredHits, 0f, false);
            timerSlider = CreateSlider("Timer", panelRoot, new Vector2(-260f, -156f), new Vector2(430f, 18f), 0f, totalDuration, totalDuration, false);

            panelRoot.gameObject.SetActive(false);
            uiBuilt = true;
        }

        private void UpdateUi()
        {
            if (!uiBuilt) return;

            if (progressSlider != null)
            {
                progressSlider.maxValue = Mathf.Max(1, requiredHits);
                progressSlider.value = hitCount;
            }

            if (timerSlider != null)
            {
                timerSlider.maxValue = totalDuration;
                timerSlider.value = Mathf.Clamp(remainingTime, 0f, totalDuration);
            }

            if (statusText != null)
            {
                statusText.text = IsRunning
                    ? $"Hits {hitCount}/{Mathf.Max(1, requiredHits)}"
                    : "Chopping completed";
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
            rect.sizeDelta = new Vector2(460f, 36f);
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

            CreateImageChild(rootRect, "Background", new Color(1f, 1f, 1f, 0.2f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fillArea = CreateRectChild(rootRect, "Fill Area", new Vector2(8f, 4f), new Vector2(-8f, -4f));
            var fill = CreateImageChild(fillArea, "Fill", new Color(0.35f, 0.9f, 0.35f, 0.95f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var handleArea = CreateRectChild(rootRect, "Handle Slide Area", new Vector2(8f, 4f), new Vector2(-8f, -4f));
            var handle = CreateImageChild(handleArea, "Handle", new Color(1f, 1f, 1f, 0.9f), new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(-9f, -9f), new Vector2(18f, 18f));

            var slider = root.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = initialValue;
            slider.interactable = interactable;
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
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
