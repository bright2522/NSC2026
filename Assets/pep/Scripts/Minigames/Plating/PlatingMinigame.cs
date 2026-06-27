using System;
using UnityEngine;
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
        [SerializeField] private PlatingUI platingUi;
        [SerializeField] private ScoringManager scoringManager;
        [SerializeField] private RecipeCatalogManager recipeCatalog;

        [Header("Settings")]
        [SerializeField] private float timeLimit = 60f;
        [SerializeField] private bool hasTimeLimit = true;
        [SerializeField] private float completeScoreThreshold = 60f;
        [SerializeField] private bool autoStartOnEnable = false;
        [SerializeField] private bool createUiOnStart = true;

        public event Action<float, bool> OnMinigameCompleted;
        public event Action<PlatingProgressResult> OnProgressChanged;

        public bool IsRunning { get; private set; }
        public float RemainingTime { get; private set; }
        public float CurrentScore => progressChecker != null ? progressChecker.LastResult?.score ?? 0f : 0f;

        private RecipeSO activeRecipe;

        private void Awake()
        {
            if (platingUi == null)
                platingUi = GetComponent<PlatingUI>();

            if (platingUi != null)
            {
                platingUi.Configure(recipeCatalog);
                platingUi.OnFinishRequested += HandleFinishRequested;
            }

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
            if (platingUi != null)
                platingUi.OnFinishRequested -= HandleFinishRequested;

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
            platingUi?.UpdateTimer(RemainingTime, hasTimeLimit);

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

            if (createUiOnStart && platingUi != null)
            {
                if (!platingUi.enabled) platingUi.enabled = true;
                platingUi.BuildRuntimeUi();
            }

            RemainingTime = Mathf.Max(1f, timeLimit);
            IsRunning = true;

            platingUi?.BindRecipe(recipe, timeLimit, hasTimeLimit);
            platingUi?.Show();
            platingUi?.SetDragging(false);

            if (progressChecker != null)
                platingUi?.UpdateProgress(progressChecker.LastResult);
        }

        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;
            SetAllItemsInteractable(false);
            platingUi?.Hide();
        }

        public void ForceComplete(float score = -1f)
        {
            if (!IsRunning) return;
            if (score < 0f) score = CurrentScore;
            Complete(score);
        }

        private void HandleFinishRequested()
        {
            if (!IsRunning) return;
            Finish();
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

            platingUi?.Hide();
            OnMinigameCompleted?.Invoke(score, success);
        }

        private void HandleItemPickedUp(DraggablePlateItem item)
        {
            platingUi?.SetDragging(true);
        }

        private void HandleItemDropped(DraggablePlateItem item)
        {
            platingUi?.SetDragging(false);
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
            platingUi?.UpdateProgress(result);
        }

        private void SetAllItemsInteractable(bool value)
        {
            if (catalogManager == null) return;
            foreach (var item in catalogManager.SpawnedTrayItems)
                item?.SetInteractable(value);
        }
    }
}
