using System;
using System.Collections.Generic;
using Pep.Recipe;
using UnityEngine;
using UnityEngine.UI;

namespace Pep.Minigames.Plating
{
    public class PlatingUI : MonoBehaviour
    {
        [SerializeField] private bool createUiOnStart = true;
        [SerializeField] private bool showFinishButton = true;
        [SerializeField] private RecipeCatalogManager recipeCatalog;

        public event Action OnFinishRequested;

        private bool uiBuilt;
        private Canvas rootCanvas;
        private RectTransform panelRoot;
        private RectTransform checklistRoot;
        private Text titleText;
        private Text timerText;
        private Text scoreText;
        private Text statusText;
        private Text hintText;
        private Image progressFill;
        private Image timerFill;
        private Button finishButton;

        private readonly List<ChecklistRow> checklistRows = new List<ChecklistRow>();
        private float timerMax = 60f;

        private class ChecklistRow
        {
            public string ingredientId;
            public string displayName;
            public Text label;
            public Image icon;
        }

        public void Configure(RecipeCatalogManager catalog)
        {
            recipeCatalog = catalog;
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

            panelRoot = CreatePanel("PlatingPanel", rootCanvas.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(860f, 210f), new Vector2(0f, -12f),
                new Color(0f, 0f, 0f, 0.62f));

            titleText = CreateText("Title", panelRoot, "Plating", 30, new Vector2(0f, -24f), 760f, TextAnchor.MiddleCenter);
            timerText = CreateText("Timer", panelRoot, "Time: 60s", 22, new Vector2(-300f, -62f), 220f, TextAnchor.MiddleLeft);
            scoreText = CreateText("Score", panelRoot, "Score: 0", 22, new Vector2(300f, -62f), 220f, TextAnchor.MiddleRight);
            statusText = CreateText("Status", panelRoot, "Drag items onto the plate", 20, new Vector2(0f, -62f), 360f, TextAnchor.MiddleCenter);

            timerFill = CreateBar("TimerBar", panelRoot, new Vector2(0f, -88f), new Vector2(780f, 10f),
                new Color(1f, 1f, 1f, 0.12f), new Color(0.95f, 0.75f, 0.2f, 0.95f));
            progressFill = CreateBar("ProgressBar", panelRoot, new Vector2(0f, -104f), new Vector2(780f, 14f),
                new Color(1f, 1f, 1f, 0.12f), new Color(0.35f, 0.88f, 0.45f, 0.95f));

            var checklistPanel = CreatePanel("ChecklistPanel", panelRoot,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-24f, 72f), new Vector2(0f, 12f),
                new Color(1f, 1f, 1f, 0.06f));
            checklistRoot = checklistPanel;
            var checklistLayout = checklistPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            checklistLayout.childAlignment = TextAnchor.UpperLeft;
            checklistLayout.spacing = 4f;
            checklistLayout.padding = new RectOffset(6, 6, 4, 4);
            checklistLayout.childControlWidth = true;
            checklistLayout.childControlHeight = true;
            checklistLayout.childForceExpandWidth = true;
            checklistLayout.childForceExpandHeight = false;

            hintText = CreateText("Hint", panelRoot, "Hold placed item to remove", 16,
                new Vector2(0f, -188f), 760f, TextAnchor.MiddleCenter);
            hintText.color = new Color(1f, 1f, 1f, 0.55f);

            if (showFinishButton)
            {
                finishButton = CreateButton("FinishButton", panelRoot, "Finish Plating",
                    new Vector2(320f, -150f), new Vector2(200f, 44f), OnFinishClicked);
            }

            panelRoot.gameObject.SetActive(false);
            uiBuilt = true;
        }

        public void Show()
        {
            if (createUiOnStart && !uiBuilt) BuildRuntimeUi();
            if (panelRoot != null) panelRoot.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.gameObject.SetActive(false);
        }

        public void BindRecipe(RecipeSO recipe, float timeLimit, bool hasTimeLimit)
        {
            if (createUiOnStart && !uiBuilt) BuildRuntimeUi();

            timerMax = Mathf.Max(1f, timeLimit);

            if (titleText != null)
                titleText.text = recipe != null ? recipe.DisplayName : "Plating";

            if (statusText != null)
                statusText.text = "Drag items onto the plate";

            if (timerText != null)
                timerText.text = hasTimeLimit ? $"Time: {Mathf.CeilToInt(timeLimit)}s" : "Free Play";

            if (timerFill != null)
            {
                timerFill.fillAmount = 1f;
                timerFill.transform.parent.gameObject.SetActive(hasTimeLimit);
            }

            if (progressFill != null) progressFill.fillAmount = 0f;
            if (scoreText != null) scoreText.text = "Score: 0";

            RebuildChecklist(recipe);
        }

        public void UpdateTimer(float remaining, bool hasTimeLimit)
        {
            if (!uiBuilt) return;

            if (timerText != null)
            {
                timerText.text = hasTimeLimit
                    ? $"Time: {Mathf.CeilToInt(Mathf.Max(0f, remaining))}s"
                    : "Free Play";
            }

            if (timerFill != null && hasTimeLimit && timerMax > 0f)
            {
                float target = Mathf.Clamp01(remaining / timerMax);
                LeanTween.value(timerFill.gameObject, timerFill.fillAmount, target, 0.2f)
                    .setEaseOutQuad()
                    .setOnUpdate((float v) => timerFill.fillAmount = v);
            }
        }

        public void UpdateProgress(PlatingProgressResult result)
        {
            if (!uiBuilt || result == null) return;

            if (scoreText != null)
                scoreText.text = $"Score: {result.score:0}";

            if (statusText != null)
            {
                if (result.extraIngredientIds.Count > 0)
                    statusText.text = $"Extra items: {result.extraIngredientIds.Count}";
                else if (result.isComplete)
                    statusText.text = "Plate complete!";
                else if (result.missingIngredientIds.Count > 0)
                    statusText.text = $"Missing: {result.missingIngredientIds.Count}";
                else
                    statusText.text = "Keep plating...";
            }

            if (progressFill != null)
            {
                float target = result.requiredCount > 0
                    ? (float)result.matchedCount / result.requiredCount
                    : 0f;
                LeanTween.value(progressFill.gameObject, progressFill.fillAmount, target, 0.25f)
                    .setEaseOutQuad()
                    .setOnUpdate((float v) => progressFill.fillAmount = v);
            }

            UpdateChecklist(result);
        }

        public void SetDragging(bool isDragging)
        {
            if (!uiBuilt || hintText == null) return;
            hintText.text = isDragging
                ? "Release over the plate to drop"
                : "Hold placed item to remove";
        }

        private void RebuildChecklist(RecipeSO recipe)
        {
            ClearChecklist();

            if (checklistRoot == null || recipe == null) return;

            var ids = recipe.RequiredIngredientIds;
            if (ids == null || ids.Count == 0)
            {
                CreateChecklistRow(checklistRoot, "any", "Place any item", null, false);
                return;
            }

            for (int i = 0; i < ids.Count; i++)
            {
                string id = ids[i];
                string label = ResolveIngredientName(id);
                Sprite icon = ResolveIngredientIcon(id);
                CreateChecklistRow(checklistRoot, id, label, icon, false);
            }
        }

        private void UpdateChecklist(PlatingProgressResult result)
        {
            if (checklistRows.Count == 0 || result?.missingIngredientIds == null) return;

            foreach (var row in checklistRows)
            {
                bool done = row.ingredientId == "any"
                    ? result.isComplete
                    : !result.missingIngredientIds.Contains(row.ingredientId);
                SetRowDone(row, done);
            }
        }

        private void CreateChecklistRow(RectTransform parent, string id, string label, Sprite icon, bool done)
        {
            var rowGo = new GameObject($"Row_{id}", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            var rowRect = rowGo.GetComponent<RectTransform>();
            rowRect.SetParent(parent, false);
            rowRect.sizeDelta = new Vector2(0f, 28f);

            var layout = rowGo.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.spacing = 8f;
            layout.padding = new RectOffset(8, 8, 2, 2);
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var markGo = new GameObject("Mark", typeof(RectTransform), typeof(Image));
            var markRect = markGo.GetComponent<RectTransform>();
            markRect.SetParent(rowRect, false);
            markRect.sizeDelta = new Vector2(18f, 18f);
            var markImage = markGo.GetComponent<Image>();
            markImage.color = done ? new Color(0.35f, 0.88f, 0.45f) : new Color(1f, 1f, 1f, 0.2f);

            if (icon != null)
            {
                var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                var iconRect = iconGo.GetComponent<RectTransform>();
                iconRect.SetParent(rowRect, false);
                iconRect.sizeDelta = new Vector2(22f, 22f);
                var iconImage = iconGo.GetComponent<Image>();
                iconImage.sprite = icon;
                iconImage.preserveAspect = true;
            }

            var text = CreateText("Label", rowRect, label, 18, Vector2.zero, 220f, TextAnchor.MiddleLeft);
            text.color = done ? new Color(0.75f, 1f, 0.78f) : new Color(1f, 1f, 1f, 0.85f);

            checklistRows.Add(new ChecklistRow
            {
                ingredientId = id,
                displayName = label,
                label = text,
                icon = markImage
            });
        }

        private void SetRowDone(ChecklistRow row, bool done)
        {
            if (row.icon != null)
                row.icon.color = done ? new Color(0.35f, 0.88f, 0.45f) : new Color(1f, 1f, 1f, 0.2f);

            if (row.label != null)
            {
                row.label.color = done ? new Color(0.75f, 1f, 0.78f) : new Color(1f, 1f, 1f, 0.85f);
                row.label.text = done ? $"✓ {row.displayName}" : $"○ {row.displayName}";
            }
        }

        private void ClearChecklist()
        {
            checklistRows.Clear();
            if (checklistRoot == null) return;

            for (int i = checklistRoot.childCount - 1; i >= 0; i--)
                Destroy(checklistRoot.GetChild(i).gameObject);
        }

        private string ResolveIngredientName(string ingredientId)
        {
            if (recipeCatalog != null && recipeCatalog.TryGetIngredientById(ingredientId, out IngredientSO ingredient))
                return ingredient.DisplayName;
            return ingredientId;
        }

        private Sprite ResolveIngredientIcon(string ingredientId)
        {
            if (recipeCatalog != null && recipeCatalog.TryGetIngredientById(ingredientId, out IngredientSO ingredient))
                return ingredient.Icon;
            return null;
        }

        private void OnFinishClicked()
        {
            OnFinishRequested?.Invoke();
        }

        private static RectTransform CreatePanel(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 sizeDelta,
            Vector2 anchoredPosition,
            Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;
            go.GetComponent<Image>().color = color;
            return rect;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string content,
            int size,
            Vector2 pos,
            float width,
            TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(width, 32f);
            rect.anchoredPosition = pos;

            var text = go.GetComponent<Text>();
            text.font = GetFont();
            text.text = content;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = alignment;
            return text;
        }

        private static Image CreateBar(
            string name,
            Transform parent,
            Vector2 pos,
            Vector2 size,
            Color bgColor,
            Color fillColor)
        {
            var bgGo = new GameObject(name, typeof(RectTransform), typeof(Image));
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.SetParent(parent, false);
            bgRect.anchorMin = new Vector2(0.5f, 1f);
            bgRect.anchorMax = new Vector2(0.5f, 1f);
            bgRect.pivot = new Vector2(0.5f, 1f);
            bgRect.sizeDelta = size;
            bgRect.anchoredPosition = pos;
            bgGo.GetComponent<Image>().color = bgColor;

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.SetParent(bgRect, false);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            var fill = fillGo.GetComponent<Image>();
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0f;
            return fill;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Vector2 pos,
            Vector2 size,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.25f, 0.65f, 0.35f, 0.95f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            var text = CreateText("Label", rect, label, 20, Vector2.zero, size.x, TextAnchor.MiddleCenter);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

            return button;
        }

        private static Font GetFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                   ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
