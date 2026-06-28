using System;
using Pep.Input;
using Pep.Recipe;
using Pep.Scoring;
using UnityEngine;
using UnityEngine.UI;

namespace Pep.Minigames.Preparation
{
    public class TiltPourMinigame : MonoBehaviour
    {
        [Header("Session")]
        [SerializeField] private IngredientSO currentIngredient;
        [SerializeField] private float totalDuration = 12f;
        [SerializeField] private float pourSpeed = 65f;

        [Header("Fallback Tuning")]
        [SerializeField] private float fallbackIdealMin = 65f;
        [SerializeField] private float fallbackIdealMax = 85f;
        [SerializeField] private float fallbackFragileTiltVelocity = 2.5f;

        [Header("Runtime")]
        [SerializeField] private bool createUiOnStart = true;
        [SerializeField] private TiltPourReader tiltReader;
        [SerializeField] private ScoringManager scoringManager;

        public event Action<float, bool> OnPourCompleted;

        public bool IsRunning => isRunning;
        public float FillAmount => fillAmount;
        public float RemainingTime => remainingTime;

        private Canvas rootCanvas;
        private RectTransform panelRoot;
        private Slider fillSlider;
        private Slider timerSlider;
        private Text titleText;
        private Text statusText;
        private Text timerText;
        private Text tiltText;

        private float fillAmount;
        private float remainingTime;
        private bool isRunning;
        private bool uiBuilt;
        private bool didBreakFragile;

        private void Awake()
        {
            if (tiltReader == null)
            {
                tiltReader = GetComponent<TiltPourReader>();
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
            float rate = tiltReader != null ? tiltReader.PourRate : 0f;
            fillAmount = Mathf.Clamp(fillAmount + rate * pourSpeed * Time.deltaTime, 0f, 100f);

            if (IsFragile() && tiltReader != null && Mathf.Abs(tiltReader.TiltVelocity) > GetFragileVelocityThreshold())
            {
                didBreakFragile = true;
                Finish();
                return;
            }

            if (fillAmount >= 100f || remainingTime <= 0f)
            {
                Finish();
                return;
            }

            UpdateUi();
        }

        public void SetIngredient(IngredientSO ingredient)
        {
            currentIngredient = ingredient;
        }

        public void Begin()
        {
            if (tiltReader != null)
            {
                tiltReader.CalibrateNeutral();
            }

            fillAmount = 0f;
            remainingTime = Mathf.Max(2f, totalDuration);
            didBreakFragile = false;
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

        public void Configure(TiltPourReader reader, ScoringManager manager)
        {
            tiltReader = reader;
            scoringManager = manager;
        }

        private void Finish()
        {
            isRunning = false;
            float score = CalculateScore();
            bool success = !didBreakFragile && score >= 50f;

            if (scoringManager != null)
            {
                scoringManager.ReportStepScore("pep/TiltPour", "Tilt Pour", score);
            }

            UpdateUi();
            if (panelRoot != null) panelRoot.gameObject.SetActive(false);
            OnPourCompleted?.Invoke(score, didBreakFragile);
        }

        private float CalculateScore()
        {
            if (didBreakFragile) return 0f;

            float idealMin = GetIdealMin();
            float idealMax = GetIdealMax();

            if (fillAmount < idealMin)
            {
                return Mathf.Lerp(20f, 75f, fillAmount / Mathf.Max(0.001f, idealMin));
            }

            if (fillAmount <= idealMax)
            {
                return 100f;
            }

            float overT = (fillAmount - idealMax) / Mathf.Max(0.001f, 100f - idealMax);
            return Mathf.Lerp(75f, 10f, overT);
        }

        private bool IsFragile()
        {
            return currentIngredient != null && currentIngredient.IsFragile;
        }

        private float GetFragileVelocityThreshold()
        {
            if (currentIngredient == null) return fallbackFragileTiltVelocity;
            return Mathf.Max(0.1f, currentIngredient.FragileMaxTiltVelocity);
        }

        private float GetIdealMin()
        {
            if (currentIngredient == null) return fallbackIdealMin;
            return Mathf.Clamp(currentIngredient.IdealPourMin, 0f, 100f);
        }

        private float GetIdealMax()
        {
            if (currentIngredient == null) return fallbackIdealMax;
            return Mathf.Clamp(currentIngredient.IdealPourMax, 0f, 100f);
        }

        public void BuildRuntimeUi()
        {
            if (uiBuilt) return;

            rootCanvas = FindObjectOfType<Canvas>();
            if (rootCanvas == null)
            {
                var canvasObject = new GameObject("PepTiltPourCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                rootCanvas = canvasObject.GetComponent<Canvas>();
                rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            var panelObject = new GameObject("PepTiltPourPanel", typeof(RectTransform), typeof(Image));
            panelRoot = panelObject.GetComponent<RectTransform>();
            panelRoot.SetParent(rootCanvas.transform, false);
            panelRoot.anchorMin = new Vector2(0.5f, 0.5f);
            panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
            panelRoot.pivot = new Vector2(0.5f, 0.5f);
            panelRoot.sizeDelta = new Vector2(760f, 420f);
            panelRoot.anchoredPosition = Vector2.zero;
            panelObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.58f);

            titleText = CreateText("Title", panelRoot, "Tilt Pour", 38, new Vector2(0f, 155f));
            titleText.alignment = TextAnchor.MiddleCenter;

            statusText = CreateText("Status", panelRoot, "Ready", 22, new Vector2(0f, 100f));
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.GetComponent<RectTransform>().sizeDelta = new Vector2(680f, 52f);

            timerText = CreateText("Timer", panelRoot, "00.0s", 30, new Vector2(0f, 68f));
            timerText.alignment = TextAnchor.MiddleCenter;

            tiltText = CreateText("Tilt", panelRoot, "Tilt 0%", 24, new Vector2(0f, 28f));
            tiltText.alignment = TextAnchor.MiddleCenter;

            fillSlider = CreateSlider("FillSlider", panelRoot, new Vector2(0f, -44f), new Vector2(560f, 30f), 0f, 100f, 0f, false);
            CreateText("FillLabel", panelRoot, "Pour Amount", 22, new Vector2(0f, -14f)).alignment = TextAnchor.MiddleCenter;

            timerSlider = CreateSlider("TimerSlider", panelRoot, new Vector2(0f, -130f), new Vector2(560f, 22f), 0f, totalDuration, totalDuration, false);
            CreateText("TimerLabel", panelRoot, "Time", 18, new Vector2(0f, -102f)).alignment = TextAnchor.MiddleCenter;

            panelRoot.gameObject.SetActive(false);
            uiBuilt = true;
        }

        private void UpdateUi()
        {
            if (!uiBuilt) return;

            if (fillSlider != null)
            {
                fillSlider.value = fillAmount;
            }

            if (timerSlider != null)
            {
                timerSlider.maxValue = totalDuration;
                timerSlider.value = Mathf.Clamp(remainingTime, 0f, totalDuration);
            }

            if (timerText != null)
            {
                timerText.text = $"{Mathf.Clamp(remainingTime, 0f, totalDuration):00.0}s";
            }

            if (tiltText != null)
            {
                float tiltPercent = tiltReader != null ? tiltReader.PourRate * 100f : 0f;
                string mouseHeld = UnityEngine.Input.GetMouseButton(0) ? " [LMB]" : "";
                tiltText.text = $"Tilt {tiltPercent:0}%{mouseHeld}";
            }

            if (titleText != null)
            {
                titleText.text = currentIngredient != null ? $"Tilt Pour - {currentIngredient.DisplayName}" : "Tilt Pour";
            }

            if (statusText != null)
            {
                if (isRunning)
                {
                    string action = IsFragile() ? "Pour slowly to avoid break" : "Hold steady tilt in ideal zone";
                    statusText.text = $"{action}\n[PC: hold LMB — mouse right = pour more]";
                }
                else if (didBreakFragile)
                {
                    statusText.text = "Broken";
                }
                else
                {
                    statusText.text = "Completed";
                }
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
            rect.sizeDelta = new Vector2(640f, 36f);
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
            var fill = CreateImageChild(fillArea, "Fill", new Color(0.2f, 0.8f, 0.3f, 0.95f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
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
