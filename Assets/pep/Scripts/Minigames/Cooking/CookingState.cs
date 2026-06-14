using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pep.Minigames.Cooking
{
    public class CookingState : MonoBehaviour
    {
        [Header("Session")]
        [SerializeField] private float totalDuration = 30f;
        [SerializeField] private float targetCookValue = 100f;

        [Header("Heat")]
        [SerializeField] private float lowHeatPerSecond = 1.2f;
        [SerializeField] private float optimalHeatPerSecond = 3.4f;
        [SerializeField] private float highHeatPerSecond = 2.1f;
        [SerializeField] private float burnPenaltyPerSecond = 4.8f;

        [Header("Shake")]
        [SerializeField] private float requiredShakePerSecond = 1.35f;
        [SerializeField] private float shakeValuePerSecond = 2.3f;

        [Header("Runtime UI")]
        [SerializeField] private bool createUiOnStart = true;

        public event Action<float, bool> OnCookingCompleted;

        public float RemainingTime => remainingTime;
        public float CookValue => cookValue;
        public float BurnValue => burnValue;
        public float HeatValue => heatSlider != null ? heatSlider.value : 0.5f;
        public bool IsRunning => isRunning;

        private Canvas rootCanvas;
        private RectTransform panelRoot;
        private Slider heatSlider;
        private Slider cookProgressSlider;
        private Slider timerSlider;
        private Text timerLabel;
        private Text statusLabel;

        private float remainingTime;
        private float cookValue;
        private float burnValue;
        private float shakeInputValue;
        private bool isRunning;
        private bool uiBuilt;

        private void Start()
        {
            if (createUiOnStart) BuildRuntimeUi();
            BeginCooking();
        }

        private void Update()
        {
            if (!isRunning) return;

            remainingTime -= Time.deltaTime;
            CollectShakeInput();
            UpdateCookingValues();
            UpdateUi();

            if (cookValue >= targetCookValue || remainingTime <= 0f)
            {
                FinishCooking();
            }
        }

        public void BeginCooking()
        {
            remainingTime = Mathf.Max(1f, totalDuration);
            cookValue = 0f;
            burnValue = 0f;
            shakeInputValue = 0f;
            isRunning = true;
            UpdateUi();
        }

        public void StopCooking()
        {
            isRunning = false;
            UpdateUi();
        }

        public void BuildRuntimeUi()
        {
            if (uiBuilt) return;

            rootCanvas = FindObjectOfType<Canvas>();
            if (rootCanvas == null)
            {
                var canvasObject = new GameObject("PepCookingCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                rootCanvas = canvasObject.GetComponent<Canvas>();
                rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            var panelObject = new GameObject("PepCookingPanel", typeof(RectTransform), typeof(Image));
            panelRoot = panelObject.GetComponent<RectTransform>();
            panelRoot.SetParent(rootCanvas.transform, false);
            panelRoot.anchorMin = new Vector2(0.5f, 0.5f);
            panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
            panelRoot.pivot = new Vector2(0.5f, 0.5f);
            panelRoot.sizeDelta = new Vector2(740f, 420f);
            panelRoot.anchoredPosition = Vector2.zero;

            var panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.55f);

            var title = CreateText("Title", panelRoot, "Cooking State", 40, new Vector2(0f, 162f));
            title.alignment = TextAnchor.MiddleCenter;

            statusLabel = CreateText("StatusLabel", panelRoot, "Preparing...", 28, new Vector2(0f, 112f));
            statusLabel.alignment = TextAnchor.MiddleCenter;

            timerLabel = CreateText("TimerLabel", panelRoot, "00.0s", 34, new Vector2(0f, 68f));
            timerLabel.alignment = TextAnchor.MiddleCenter;

            heatSlider = CreateSlider("HeatSlider", panelRoot, new Vector2(0f, 16f), new Vector2(560f, 30f), 0f, 1f, 0.5f, true);
            CreateText("HeatText", panelRoot, "Gas Fire", 24, new Vector2(0f, 44f)).alignment = TextAnchor.MiddleCenter;

            cookProgressSlider = CreateSlider("CookProgressSlider", panelRoot, new Vector2(0f, -72f), new Vector2(560f, 30f), 0f, targetCookValue, 0f, false);
            CreateText("CookText", panelRoot, "Cook Progress", 24, new Vector2(0f, -44f)).alignment = TextAnchor.MiddleCenter;

            timerSlider = CreateSlider("TimerSlider", panelRoot, new Vector2(0f, -160f), new Vector2(560f, 22f), 0f, totalDuration, totalDuration, false);
            CreateText("TimeBarText", panelRoot, "Timer", 20, new Vector2(0f, -132f)).alignment = TextAnchor.MiddleCenter;

            uiBuilt = true;
        }

        private void CollectShakeInput()
        {
            var acceleration = Input.acceleration;
            var sensorMagnitude = acceleration.magnitude;
            if (sensorMagnitude < 0.01f) sensorMagnitude = 0f;

            float keyboardShake = 0f;
            if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
            {
                keyboardShake = 1f;
            }

            shakeInputValue = Mathf.Max(sensorMagnitude, keyboardShake);
        }

        private void UpdateCookingValues()
        {
            float dt = Time.deltaTime;
            float heat = HeatValue;
            float cookDelta;

            if (heat < 0.33f)
            {
                cookDelta = lowHeatPerSecond * dt;
            }
            else if (heat <= 0.7f)
            {
                cookDelta = optimalHeatPerSecond * dt;
            }
            else
            {
                cookDelta = highHeatPerSecond * dt;
                burnValue += burnPenaltyPerSecond * (heat - 0.7f) * dt;
            }

            float shakeBonus = 0f;
            if (shakeInputValue >= requiredShakePerSecond)
            {
                shakeBonus = shakeValuePerSecond * dt;
            }
            else if (shakeInputValue > 0f)
            {
                shakeBonus = (shakeInputValue / requiredShakePerSecond) * shakeValuePerSecond * 0.6f * dt;
            }

            cookValue = Mathf.Clamp(cookValue + cookDelta + shakeBonus, 0f, targetCookValue);
            burnValue = Mathf.Clamp(burnValue, 0f, targetCookValue);
        }

        private void FinishCooking()
        {
            isRunning = false;
            float cookedRatio = targetCookValue <= 0f ? 0f : cookValue / targetCookValue;
            float burnRatio = targetCookValue <= 0f ? 0f : burnValue / targetCookValue;
            float score = Mathf.Clamp01(cookedRatio - burnRatio * 0.65f) * 100f;
            bool success = score >= 55f;

            OnCookingCompleted?.Invoke(score, success);
            UpdateUi();
        }

        private void UpdateUi()
        {
            if (!uiBuilt) return;

            if (cookProgressSlider != null)
            {
                cookProgressSlider.maxValue = targetCookValue;
                cookProgressSlider.value = cookValue;
            }

            if (timerSlider != null)
            {
                timerSlider.maxValue = totalDuration;
                timerSlider.value = Mathf.Clamp(remainingTime, 0f, totalDuration);
            }

            if (timerLabel != null)
            {
                timerLabel.text = $"{Mathf.Clamp(remainingTime, 0f, totalDuration):00.0}s";
            }

            if (statusLabel != null)
            {
                if (isRunning)
                {
                    statusLabel.text = $"Cook {cookValue:0}/{targetCookValue:0}  Burn {burnValue:0}";
                }
                else
                {
                    statusLabel.text = "Cooking Completed";
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
            textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComp.text = text;
            textComp.fontSize = size;
            textComp.color = Color.white;
            return textComp;
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

            var bg = CreateImageChild(rootRect, "Background", new Color(1f, 1f, 1f, 0.25f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
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
