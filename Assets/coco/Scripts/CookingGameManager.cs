using System.Collections.Generic;
using UnityEngine;

namespace CookingGame
{
    public class CookingGameManager : MonoBehaviour
    {
        public enum GameState
        {
            SelectRecipe,
            PreStepIntro,
            PlayingMinigame,
            StepResult,
            RecipeComplete
        }

        [Header("State")]
        public GameState currentState = GameState.SelectRecipe;
        public Recipe currentRecipe;
        public int currentStepIndex = 0;
        public float totalRecipeScore = 0f;
        private List<float> stepScores = new List<float>();

        [Header("Minigame Controllers")]
        public ChoppingMinigame choppingController;
        public StirringMinigame stirringController;
        public GrillingControllerWrapper grillingController; // Can accept GrillingMinigame directly
        public GrillingMinigame grillingControllerDirect;
        public SeasoningMinigame seasoningController;
        public PoundingMinigame poundingController;

        [Header("UI Controller")]
        public CookingGameUI uiController;

        [Header("Database")]
        public RecipeDatabase recipeDatabase;

        private float introTimer = 3f;
        private bool introCounting = false;

        private void Start()
        {
            if (recipeDatabase == null)
            {
                recipeDatabase = GetComponent<RecipeDatabase>();
                if (recipeDatabase == null)
                {
                    recipeDatabase = gameObject.AddComponent<RecipeDatabase>();
                }
            }

            // Ensure all controller references are inactive initially
            DisableAllControllers();

            // Load Menu Selection UI
            SetState(GameState.SelectRecipe);
        }

        private void Update()
        {
            if (currentState == GameState.PreStepIntro && introCounting)
            {
                introTimer -= Time.deltaTime;
                if (uiController != null)
                {
                    uiController.UpdateIntroCountdown(Mathf.CeilToInt(introTimer));
                }

                if (introTimer <= 0f)
                {
                    introCounting = false;
                    StartActiveStep();
                }
            }
        }

        public void SetState(GameState newState)
        {
            currentState = newState;
            if (uiController != null)
            {
                uiController.OnStateChanged(currentState);
            }

            switch (currentState)
            {
                case GameState.SelectRecipe:
                    currentRecipe = null;
                    currentStepIndex = 0;
                    totalRecipeScore = 0f;
                    stepScores.Clear();
                    DisableAllControllers();
                    break;

                case GameState.PreStepIntro:
                    introTimer = 3.5f; // Extra half second for UI delay
                    introCounting = true;
                    DisableAllControllers();
                    break;

                case GameState.PlayingMinigame:
                    // Handled inside StartActiveStep()
                    break;

                case GameState.StepResult:
                    DisableAllControllers();
                    break;

                case GameState.RecipeComplete:
                    DisableAllControllers();
                    CalculateFinalScore();
                    break;
            }
        }

        public void SelectRecipe(int index)
        {
            if (recipeDatabase == null) return;
            Recipe selected = recipeDatabase.GetRecipe(index);
            if (selected != null)
            {
                currentRecipe = selected;
                currentStepIndex = 0;
                totalRecipeScore = 0f;
                stepScores.Clear();
                
                Debug.Log($"[GameManager] Selected Recipe: {currentRecipe.recipeNameThai}");
                SetState(GameState.PreStepIntro);
            }
        }

        private void StartActiveStep()
        {
            if (currentRecipe == null || currentStepIndex >= currentRecipe.steps.Count)
            {
                SetState(GameState.RecipeComplete);
                return;
            }

            SetState(GameState.PlayingMinigame);
            RecipeStep step = currentRecipe.steps[currentStepIndex];
            Debug.Log($"[GameManager] Starting step {currentStepIndex + 1}/{currentRecipe.steps.Count}: {step.instructionThai} ({step.minigameType})");

            if (uiController != null)
            {
                uiController.SetupGameplayHUD(step, currentStepIndex + 1, currentRecipe.steps.Count);
            }

            // Launch corresponding minigame
            DisableAllControllers();
            switch (step.minigameType)
            {
                case MinigameType.Chopping:
                    if (choppingController != null)
                    {
                        choppingController.StartMinigame(step, this);
                    }
                    else
                    {
                        Debug.LogError("Chopping Controller is missing!");
                        OnStepCompleted(50f); // Fallback
                    }
                    break;

                case MinigameType.Stirring:
                    if (stirringController != null)
                    {
                        stirringController.StartMinigame(step, this);
                    }
                    else
                    {
                        Debug.LogError("Stirring Controller is missing!");
                        OnStepCompleted(50f);
                    }
                    break;

                case MinigameType.Grilling:
                    GrillingMinigame griller = grillingControllerDirect != null ? grillingControllerDirect : (grillingController != null ? grillingController.minigame : null);
                    if (griller != null)
                    {
                        griller.StartMinigame(step, this);
                    }
                    else
                    {
                        Debug.LogError("Grilling Controller is missing!");
                        OnStepCompleted(50f);
                    }
                    break;

                case MinigameType.Seasoning:
                    if (seasoningController != null)
                    {
                        seasoningController.StartMinigame(step, this);
                    }
                    else
                    {
                        Debug.LogError("Seasoning Controller is missing!");
                        OnStepCompleted(50f);
                    }
                    break;

                case MinigameType.Pounding:
                    if (poundingController != null)
                    {
                        poundingController.StartMinigame(step, this);
                    }
                    else
                    {
                        Debug.LogError("Pounding Controller is missing!");
                        OnStepCompleted(50f);
                    }
                    break;
            }
        }

        public void OnStepCompleted(float score)
        {
            Debug.Log($"[GameManager] Step {currentStepIndex + 1} finished with score: {score}");
            stepScores.Add(score);
            
            // Show brief result screen
            SetState(GameState.StepResult);
            
            if (uiController != null)
            {
                uiController.ShowStepResult(score, currentRecipe.steps[currentStepIndex].instructionThai);
            }
        }

        public void LoadNextStep()
        {
            currentStepIndex++;
            if (currentRecipe != null && currentStepIndex < currentRecipe.steps.Count)
            {
                SetState(GameState.PreStepIntro);
            }
            else
            {
                SetState(GameState.RecipeComplete);
            }
        }

        private void CalculateFinalScore()
        {
            if (stepScores.Count == 0)
            {
                totalRecipeScore = 0f;
                return;
            }

            float sum = 0f;
            foreach (float s in stepScores)
            {
                sum += s;
            }
            totalRecipeScore = sum / stepScores.Count;

            string medal = GetRecipeMedal(totalRecipeScore);
            Debug.Log($"[GameManager] Recipe Complete! Final Score: {totalRecipeScore:F1} ({medal} Medal)");

            if (uiController != null)
            {
                uiController.ShowFinalRecipeResult(currentRecipe, totalRecipeScore, medal);
            }
        }

        public string GetRecipeMedal(float score)
        {
            if (score >= 90f) return "Gold";     // Perfect Cooking Chef!
            if (score >= 70f) return "Silver";   // Good Home Cook!
            if (score >= 50f) return "Bronze";   // OK Kitchen Assistant!
            return "Fail";                      // Needs practice!
        }

        private void DisableAllControllers()
        {
            if (choppingController != null) choppingController.gameObject.SetActive(false);
            if (stirringController != null) stirringController.gameObject.SetActive(false);
            
            GrillingMinigame griller = grillingControllerDirect != null ? grillingControllerDirect : (grillingController != null ? grillingController.minigame : null);
            if (griller != null) griller.gameObject.SetActive(false);
            
            if (seasoningController != null) seasoningController.gameObject.SetActive(false);
            if (poundingController != null) poundingController.gameObject.SetActive(false);
        }

        public void RestartGame()
        {
            SetState(GameState.SelectRecipe);
        }
    }

    [System.Serializable]
    public class GrillingControllerWrapper
    {
        public GrillingMinigame minigame;
    }
}
