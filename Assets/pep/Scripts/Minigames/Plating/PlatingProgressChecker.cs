using System;
using System.Collections.Generic;
using UnityEngine;
using Pep.Recipe;

namespace Pep.Minigames.Plating
{
    [Serializable]
    public class PlatingProgressResult
    {
        public float score;
        public int requiredCount;
        public int matchedCount;
        public List<string> missingIngredientIds;
        public List<string> extraIngredientIds;
        public bool isComplete;

        public PlatingProgressResult()
        {
            missingIngredientIds = new List<string>();
            extraIngredientIds = new List<string>();
        }
    }

    public class PlatingProgressChecker : MonoBehaviour
    {
        [SerializeField] private PlateDropZone dropZone;
        [SerializeField] private float extraItemPenaltyPerItem = 5f;
        [SerializeField] private bool requireAllIngredients = true;

        public event Action<PlatingProgressResult> OnProgressUpdated;

        private RecipeSO targetRecipe;

        public PlatingProgressResult LastResult { get; private set; }

        private void Awake()
        {
            if (dropZone != null)
                dropZone.OnPlateChanged += EvaluateProgress;
        }

        private void OnDestroy()
        {
            if (dropZone != null)
                dropZone.OnPlateChanged -= EvaluateProgress;
        }

        public void SetTargetRecipe(RecipeSO recipe)
        {
            targetRecipe = recipe;
            EvaluateProgress();
        }

        public void SetDropZone(PlateDropZone zone)
        {
            if (dropZone != null)
                dropZone.OnPlateChanged -= EvaluateProgress;

            dropZone = zone;

            if (dropZone != null)
                dropZone.OnPlateChanged += EvaluateProgress;
        }

        public PlatingProgressResult EvaluateNow()
        {
            EvaluateProgress();
            return LastResult;
        }

        private void EvaluateProgress()
        {
            var result = new PlatingProgressResult();

            if (dropZone == null)
            {
                LastResult = result;
                OnProgressUpdated?.Invoke(result);
                return;
            }

            List<string> placedIds = dropZone.GetPlacedIngredientIds();

            if (targetRecipe == null)
            {
                result.score = placedIds.Count > 0 ? 50f : 0f;
                result.isComplete = placedIds.Count > 0;
                LastResult = result;
                OnProgressUpdated?.Invoke(result);
                return;
            }

            var required = new List<string>(targetRecipe.RequiredIngredientIds);
            var remaining = new List<string>(required);
            var extras = new List<string>();

            result.requiredCount = required.Count;

            foreach (string placedId in placedIds)
            {
                if (remaining.Contains(placedId))
                {
                    remaining.Remove(placedId);
                    result.matchedCount++;
                }
                else
                {
                    extras.Add(placedId);
                }
            }

            result.missingIngredientIds = remaining;
            result.extraIngredientIds = extras;

            float baseScore = result.requiredCount > 0
                ? (float)result.matchedCount / result.requiredCount * 100f
                : 100f;

            float penalty = extras.Count * extraItemPenaltyPerItem;
            result.score = Mathf.Clamp(baseScore - penalty, 0f, 100f);

            result.isComplete = requireAllIngredients
                ? result.matchedCount >= result.requiredCount
                : result.matchedCount > 0;

            LastResult = result;
            OnProgressUpdated?.Invoke(result);
        }

        public float GetScoreNow()
        {
            EvaluateProgress();
            return LastResult?.score ?? 0f;
        }
    }
}
