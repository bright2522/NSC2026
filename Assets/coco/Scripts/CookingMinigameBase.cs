using UnityEngine;

namespace CookingGame
{
    public abstract class CookingMinigameBase : MonoBehaviour
    {
        [Header("Minigame State")]
        public bool isActive = false;
        public bool isCompleted = false;
        public float timeLimit = 15f;
        public float timeRemaining = 15f;
        
        [Tooltip("Score for this step (typically 0 to 100)")]
        public float stepScore = 0f;

        protected RecipeStep currentStep;
        protected CookingGameManager gameManager;

        public virtual void StartMinigame(RecipeStep step, CookingGameManager manager)
        {
            currentStep = step;
            gameManager = manager;
            
            timeLimit = step.timeLimit;
            timeRemaining = timeLimit;
            stepScore = 0f;
            isActive = true;
            isCompleted = false;

            gameObject.SetActive(true);
            OnMinigameStart();
        }

        protected virtual void Update()
        {
            if (!isActive || isCompleted) return;

            timeRemaining -= Time.deltaTime;
            OnMinigameUpdate();

            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                TimeOut();
            }
        }

        // Hook for subclass specific startup
        protected abstract void OnMinigameStart();

        // Hook for subclass specific update
        protected abstract void OnMinigameUpdate();

        protected virtual void TimeOut()
        {
            // Time is up! End the minigame with current score
            EndMinigame(stepScore >= 50f, stepScore);
        }

        public virtual void EndMinigame(bool success, float finalScore)
        {
            if (isCompleted) return;
            
            isActive = false;
            isCompleted = true;
            stepScore = Mathf.Clamp(finalScore, 0f, 100f);

            // Report results to CookingGameManager
            if (gameManager != null)
            {
                gameManager.OnStepCompleted(stepScore);
            }

            OnMinigameEnd();
            gameObject.SetActive(false);
        }

        // Hook for subclass cleanup
        protected virtual void OnMinigameEnd() { }

        // Standard rating calculation for feedback
        public string GetPerformanceRating(float score)
        {
            if (score >= 90f) return "Perfect!";
            if (score >= 70f) return "Good!";
            if (score >= 50f) return "OK!";
            return "Oops!";
        }
    }
}
