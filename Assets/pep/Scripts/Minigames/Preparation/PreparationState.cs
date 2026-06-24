using System;
using Pep.Scoring;
using UnityEngine;
using UnityEngine.UI;

namespace Pep.Minigames.Preparation
{
    public class PreparationState : MonoBehaviour
    {
        [SerializeField] private TiltPourMinigame tiltPourMinigame;
        [SerializeField] private ScoringManager scoringManager;
        [SerializeField] private bool createUiOnStart = true;
        [SerializeField] private bool autoStartOnEnable = false;
        [SerializeField] private float fallbackStepScore = 55f;

        public event Action<float, bool> OnPreparationCompleted;

        public bool IsRunning { get; private set; }
        public bool LastBroken { get; private set; }
        public float LastScore { get; private set; }

        private Canvas rootCanvas;
        private RectTransform panelRoot;
        private Text titleText;
        private Text statusText;
        private bool uiBuilt;

        private void Awake()
        {
            if (tiltPourMinigame == null)
            {
                tiltPourMinigame = GetComponent<TiltPourMinigame>();
            }
        }

        private void OnEnable()
        {
            if (autoStartOnEnable)
            {
                Begin();
            }
        }

        public void Configure(TiltPourMinigame minigame, ScoringManager manager)
        {
            tiltPourMinigame = minigame;
            scoringManager = manager;
        }

        public void Begin()
        {
            if (createUiOnStart && !uiBuilt)
            {
                BuildRuntimeUi();
            }

            IsRunning = true;
            LastBroken = false;
            LastScore = 0f;
            if (panelRoot != null) panelRoot.gameObject.SetActive(true);
            UpdateUi("Preparation started");

            if (tiltPourMinigame == null)
            {
                Complete(fallbackStepScore, false);
                return;
            }

            tiltPourMinigame.OnPourCompleted -= HandleTiltCompleted;
            tiltPourMinigame.OnPourCompleted += HandleTiltCompleted;
            tiltPourMinigame.enabled = true;
            tiltPourMinigame.Begin();
        }

        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;
            if (tiltPourMinigame != null) tiltPourMinigame.OnPourCompleted -= HandleTiltCompleted;
            if (panelRoot != null) panelRoot.gameObject.SetActive(false);
        }

        public void ForceComplete(float score = 70f)
        {
            if (!IsRunning) return;
            Complete(score, false);
        }

        private void HandleTiltCompleted(float score, bool broken)
        {
            tiltPourMinigame.OnPourCompleted -= HandleTiltCompleted;
            Complete(score, broken);
        }

        private void Complete(float score, bool broken)
        {
            if (!IsRunning) return;

            IsRunning = false;
            LastBroken = broken;
            LastScore = Mathf.Clamp(score, 0f, 100f);

            if (scoringManager != null && tiltPourMinigame == null)
            {
                scoringManager.ReportStepScore("pep/PreparationState", "Preparation", LastScore);
            }

            UpdateUi(broken ? "Preparation failed" : "Preparation completed");
            if (panelRoot != null) panelRoot.gameObject.SetActive(false);
            OnPreparationCompleted?.Invoke(LastScore, !broken);
        }

        public void BuildRuntimeUi()
        {
            if (uiBuilt) return;

            rootCanvas = FindObjectOfType<Canvas>();
            if (rootCanvas == null)
            {
                var canvasObject = new GameObject("PepPreparationCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                rootCanvas = canvasObject.GetComponent<Canvas>();
                rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            var panelObject = new GameObject("PepPreparationPanel", typeof(RectTransform), typeof(Image));
            panelRoot = panelObject.GetComponent<RectTransform>();
            panelRoot.SetParent(rootCanvas.transform, false);
            panelRoot.anchorMin = new Vector2(0f, 1f);
            panelRoot.anchorMax = new Vector2(0f, 1f);
            panelRoot.pivot = new Vector2(0f, 1f);
            panelRoot.sizeDelta = new Vector2(520f, 120f);
            panelRoot.anchoredPosition = new Vector2(24f, -24f);
            panelObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            titleText = CreateText("Title", panelRoot, "Preparation", 30, new Vector2(250f, -34f));
            titleText.alignment = TextAnchor.MiddleCenter;

            statusText = CreateText("Status", panelRoot, "Idle", 22, new Vector2(250f, -82f));
            statusText.alignment = TextAnchor.MiddleCenter;

            panelRoot.gameObject.SetActive(false);
            uiBuilt = true;
        }

        private void UpdateUi(string status)
        {
            if (!uiBuilt) return;
            if (statusText != null) statusText.text = status;
        }

        private Text CreateText(string name, RectTransform parent, string text, int size, Vector2 anchoredPosition)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(480f, 40f);
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
    }
}
