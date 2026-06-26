using System;
using UnityEngine;
using UnityEngine.UI;
using Pep.Recipe;
using Pep.Scoring;

namespace Pep.Minigames.Plating
{
    public class PlatingMinigame : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlatingItemCatalogManager catalogManager;
        [SerializeField] private PlateDropZone dropZone;
        [SerializeField] private PlatingProgressChecker progressChecker;
        [SerializeField] private ScoringManager scoringManager;

        [Header("Settings")]
        [SerializeField] private float timeLimit = 60f;
        [SerializeField] private bool hasTimeLimit = true;
        [SerializeField] private float completeScoreThreshold = 60f;
        [SerializeField] private bool autoStartOnEnable = false;
        [SerializeField] private bool createUiOnStart = true;

        [Header("Plate Highlight")]
        [SerializeField] private Renderer plateRenderer;
        [SerializeField] private Color idleColor = Color.white;
        [SerializeField] private Color hoverColor = new Color(0.6f, 1f, 0.6f);

        public event Action<float, bool> OnMinigameCompleted;
        public event Action<PlatingProgressResult> OnProgressChanged;

        public bool IsRunning { get; private set; }
        public float RemainingTime { get; private set; }
        public float CurrentScore => progressChecker != null ? progressChecker.LastResult?.score ?? 0f : 0f;

        private RecipeSO activeRecipe;
        private bool uiBuilt;
        private Canvas rootCanvas;
        private RectTransform panelRoot;
        private Text timerText;
        private Text progressText;
        private Text scoreText;
        private Image progressBar;
        private bool isDraggingAnyItem;

        private void Awake()
        {
            if (catalogManager != null)
            {
                catalogManager.OnItemPickedUp += HandleItemPickedUp;
                catalogManager.OnItemDropped += HandleItemDropped;
            }

            if (progressChecker != null)
            {
                progressChecker.SetDropZone(dropZone);
                progressChecker.OnProgressUpdated += HandleProgressUpdated;
            }

            if (dropZone != null)
                dropZone.OnItemPlaced += HandleItemPlaced;
        }

        private void OnDestroy()
        {
            if (catalogManager != null)
            {
                catalogManager.OnItemPickedUp -= HandleItemPickedUp;
                catalogManager.OnItemDropped -= HandleItemDropped;
            }

            if (progressChecker != null)
                progressChecker.OnProgressUpdated -= HandleProgressUpdated;

            if (dropZone != null)
                dropZone.OnItemPlaced -= HandleItemPlaced;
        }

        private void OnEnable()
        {
            if (autoStartOnEnable) Begin(activeRecipe);
        }

        private void Update()
        {
            if (!IsRunning) return;
            if (!hasTimeLimit) return;

            RemainingTime -= Time.deltaTime;
            UpdateUi();

            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                Finish();
            }
        }

        public void Configure(ScoringManager manager)
        {
            scoringManager = manager;
        }

        public void Begin(RecipeSO recipe)
        {
            activeRecipe = recipe;

            if (progressChecker != null)
                progressChecker.SetTargetRecipe(recipe);

            if (dropZone != null)
                dropZone.ClearPlate();

            if (catalogManager != null)
            {
                catalogManager.ResetPlacedCounts();
                catalogManager.SpawnTray();
            }

            if (createUiOnStart && !uiBuilt)
                BuildRuntimeUi();

            RemainingTime = Mathf.Max(1f, timeLimit);
            IsRunning = true;

            if (panelRoot != null) panelRoot.gameObject.SetActive(true);
            UpdateUi();
            SetPlateColor(idleColor);
        }

        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;
            SetAllItemsInteractable(false);
            if (panelRoot != null) panelRoot.gameObject.SetActive(false);
        }

        public void ForceComplete(float score = -1f)
        {
            if (!IsRunning) return;
            if (score < 0f) score = CurrentScore;
            Complete(score);
        }

        private void Finish()
        {
            Complete(CurrentScore);
        }

        private void Complete(float score)
        {
            IsRunning = false;
            bool success = score >= completeScoreThreshold;
            SetAllItemsInteractable(false);

            if (scoringManager != null)
                scoringManager.ReportStepScore("pep/Plating", "Plating", score);

            if (panelRoot != null) panelRoot.gameObject.SetActive(false);
            OnMinigameCompleted?.Invoke(score, success);
        }

        private void HandleItemPickedUp(DraggablePlateItem item)
        {
            isDraggingAnyItem = true;
            SetPlateColor(hoverColor);
        }

        private void HandleItemDropped(DraggablePlateItem item)
        {
            isDraggingAnyItem = false;
            SetPlateColor(idleColor);
        }

        private void HandleItemPlaced(DraggablePlateItem item)
        {
            LeanTween.scale(item.gameObject, item.transform.localScale * 1.05f, 0.06f)
                .setEaseOutQuad()
                .setLoopPingPong(1);
        }

        private void HandleProgressUpdated(PlatingProgressResult result)
        {
            OnProgressChanged?.Invoke(result);
            UpdateProgressUi(result);
        }

        private void SetPlateColor(Color color)
        {
            if (plateRenderer == null) return;
            LeanTween.value(plateRenderer.gameObject,
                plateRenderer.material.color, color, 0.15f)
                .setOnUpdate((Color c) => plateRenderer.material.color = c);
        }

        private void SetAllItemsInteractable(bool value)
        {
            if (catalogManager == null) return;
            foreach (var item in catalogManager.SpawnedTrayItems)
                item?.SetInteractable(value);
        }

        public void BuildRuntimeUi()
        {
            if (uiBuilt) return;

            rootCanvas = FindObjectOfType<Canvas>();
            if (rootCanvas == null)
            {
                var canvasGo = new GameObject("PlatingCanvas",
                    typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                rootCanvas = canvasGo.GetComponent<Canvas>();
                rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            var panelGo = new GameObject("PlatingPanel", typeof(RectTransform), typeof(Image));
            panelRoot = panelGo.GetComponent<RectTransform>();
            panelRoot.SetParent(rootCanvas.transform, false);
            panelRoot.anchorMin = new Vector2(0.5f, 1f);
            panelRoot.anchorMax = new Vector2(0.5f, 1f);
            panelRoot.pivot = new Vector2(0.5f, 1f);
            panelRoot.sizeDelta = new Vector2(760f, 130f);
            panelRoot.anchoredPosition = new Vector2(0f, -10f);
            panelGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            timerText = CreateText("Timer", panelRoot, "60.0", 28, new Vector2(-260f, -28f));
            progressText = CreateText("Progress", panelRoot, "0 / 0", 22, new Vector2(0f, -28f));
            scoreText = CreateText("Score", panelRoot, "Score: 0", 22, new Vector2(200f, -28f));

            var barBg = new GameObject("ProgressBarBg", typeof(RectTransform), typeof(Image));
            var bgRect = barBg.GetComponent<RectTransform>();
            bgRect.SetParent(panelRoot, false);
            bgRect.anchorMin = new Vector2(0.5f, 0f);
            bgRect.anchorMax = new Vector2(0.5f, 0f);
            bgRect.pivot = new Vector2(0.5f, 0f);
            bgRect.sizeDelta = new Vector2(680f, 22f);
            bgRect.anchoredPosition = new Vector2(0f, 12f);
            barBg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);

            var barFillGo = new GameObject("ProgressBarFill", typeof(RectTransform), typeof(Image));
            progressBar = barFillGo.GetComponent<Image>();
            var fillRect = barFillGo.GetComponent<RectTransform>();
            fillRect.SetParent(bgRect.transform, false);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.sizeDelta = new Vector2(0f, 0f);
            fillRect.anchoredPosition = Vector2.zero;
            progressBar.color = new Color(0.3f, 0.85f, 0.4f);
            progressBar.type = Image.Type.Filled;
            progressBar.fillMethod = Image.FillMethod.Horizontal;
            progressBar.fillAmount = 0f;

            panelRoot.gameObject.SetActive(false);
            uiBuilt = true;
        }

        private void UpdateUi()
        {
            if (!uiBuilt) return;

            if (timerText != null)
            {
                timerText.text = hasTimeLimit
                    ? $"Time: {Mathf.CeilToInt(RemainingTime)}s"
                    : "Free Play";
            }
        }

        private void UpdateProgressUi(PlatingProgressResult result)
        {
            if (!uiBuilt) return;

            if (progressText != null)
                progressText.text = $"Items: {result.matchedCount} / {result.requiredCount}";

            if (scoreText != null)
                scoreText.text = $"Score: {result.score:0}";

            if (progressBar != null)
            {
                float target = result.requiredCount > 0
                    ? (float)result.matchedCount / result.requiredCount
                    : 0f;
                LeanTween.value(progressBar.gameObject,
                    progressBar.fillAmount, target, 0.3f)
                    .setEaseOutQuad()
                    .setOnUpdate((float v) => progressBar.fillAmount = v);
            }
        }

        private Text CreateText(string name, RectTransform parent, string content, int size, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(220f, 40f);
            rect.anchoredPosition = pos;
            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                     ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text = content;
            t.fontSize = size;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            return t;
        }
    }
}
