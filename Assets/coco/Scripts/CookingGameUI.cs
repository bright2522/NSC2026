using UnityEngine;
using UnityEngine.UI;

namespace CookingGame
{
    public class CookingGameUI : MonoBehaviour
    {
        [Header("Manager Reference")]
        public CookingGameManager gameManager;

        [Header("UI Panels")]
        public GameObject selectRecipePanel;
        public GameObject preStepIntroPanel;
        public GameObject gameplayHUDPanel;
        public GameObject stepResultPanel;
        public GameObject recipeCompletePanel;

        [Header("Minigame Panels (Container references)")]
        [Tooltip("Active when chopping")]
        public GameObject choppingMinigameUI;
        [Tooltip("Active when stirring")]
        public GameObject stirringMinigameUI;
        [Tooltip("Active when grilling")]
        public GameObject grillingMinigameUI;
        [Tooltip("Active when seasoning")]
        public GameObject seasoningMinigameUI;
        [Tooltip("Active when pounding")]
        public GameObject poundingMinigameUI;

        [Header("Pre-Step Intro UI")]
        public Text countdownText;
        public Text upcomingStepInstructionText;
        public Text upcomingStepIngredientText;

        [Header("Gameplay HUD UI")]
        public Text stepInstructionText;
        public Text stepProgressText; // e.g. "Step 1/3"
        public Text timerText;
        public Text feedbackText; // Pop-up feedback text
        private float feedbackTimer = 0f;

        [Header("Step Result UI")]
        public Text stepResultTitleText;
        public Text stepResultScoreText;
        
        [Header("Final Recipe Result UI")]
        public Text finalRecipeNameText;
        public Text finalScoreText;
        public Text finalDescriptionText;
        public Text finalMedalRatingText; // "Gold Chef", "Silver Cook", "Bronze Assistant"
        public Image finalMedalImage; // For visual sprite if assigned
        public Color goldMedalColor = new Color(1f, 0.84f, 0f);
        public Color silverMedalColor = new Color(0.75f, 0.75f, 0.75f);
        public Color bronzeMedalColor = new Color(0.8f, 0.5f, 0.2f);
        public Color failColor = Color.red;

        [Header("Recipe Selection Scroll List")]
        public Transform recipeCardContainer;
        [Tooltip("Prefab for a Recipe select Card. Should contain text elements and a Button.")]
        public GameObject recipeCardPrefab;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = GetComponent<CookingGameManager>();
            }
        }

        private void Start()
        {
            PopulateRecipeList();
            
            if (feedbackText != null)
            {
                feedbackText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            // Handle floating feedback text fading
            if (feedbackText != null && feedbackText.gameObject.activeSelf)
            {
                feedbackTimer -= Time.deltaTime;
                
                // Animate text upwards slightly
                feedbackText.transform.Translate(Vector3.up * Time.deltaTime * 50f, Space.Self);

                // Fade alpha
                Color c = feedbackText.color;
                c.a = Mathf.Clamp01(feedbackTimer);
                feedbackText.color = c;

                if (feedbackTimer <= 0f)
                {
                    feedbackText.gameObject.SetActive(false);
                }
            }

            // Handle gameplay timer sync with active minigame
            if (gameplayHUDPanel.activeSelf && gameManager != null && gameManager.currentState == CookingGameManager.GameState.PlayingMinigame)
            {
                CookingMinigameBase activeMinigame = GetActiveMinigameScript();
                if (activeMinigame != null)
                {
                    timerText.text = $"{Mathf.CeilToInt(activeMinigame.timeRemaining)}s";
                    // Alert color if time is low
                    if (activeMinigame.timeRemaining <= 4f)
                    {
                        timerText.color = Color.red;
                        timerText.fontSize = 32; // Pulse effect
                    }
                    else
                    {
                        timerText.color = Color.white;
                        timerText.fontSize = 28;
                    }
                }
            }
        }

        private void PopulateRecipeList()
        {
            if (recipeCardContainer == null || recipeCardPrefab == null || gameManager == null || gameManager.recipeDatabase == null)
            {
                return;
            }

            // Clear container
            foreach (Transform child in recipeCardContainer)
            {
                Destroy(child.gameObject);
            }

            // Fetch pre-configured recipes
            gameManager.recipeDatabase.InitializeDatabase();
            var recipes = gameManager.recipeDatabase.recipes;

            for (int i = 0; i < recipes.Count; i++)
            {
                int index = i;
                Recipe recipe = recipes[i];
                
                GameObject card = Instantiate(recipeCardPrefab, recipeCardContainer);
                
                // Set text fields in the prefab Card
                // Assumes child objects or specific script. We can find by component/name
                Text[] texts = card.GetComponentsInChildren<Text>();
                foreach (Text textComponent in texts)
                {
                    if (textComponent.gameObject.name == "ThaiName")
                    {
                        textComponent.text = recipe.recipeNameThai;
                    }
                    else if (textComponent.gameObject.name == "EnglishName")
                    {
                        textComponent.text = recipe.recipeNameEnglish;
                    }
                    else if (textComponent.gameObject.name == "Description")
                    {
                        textComponent.text = recipe.description;
                    }
                }

                // Attach button handler
                Button btn = card.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => gameManager.SelectRecipe(index));
                }
            }
        }

        public void OnStateChanged(CookingGameManager.GameState state)
        {
            // Close all main panels
            selectRecipePanel.SetActive(false);
            preStepIntroPanel.SetActive(false);
            gameplayHUDPanel.SetActive(false);
            stepResultPanel.SetActive(false);
            recipeCompletePanel.SetActive(false);

            // Close all minigame HUD containers
            if (choppingMinigameUI != null) choppingMinigameUI.SetActive(false);
            if (stirringMinigameUI != null) stirringMinigameUI.SetActive(false);
            if (grillingMinigameUI != null) grillingMinigameUI.SetActive(false);
            if (seasoningMinigameUI != null) seasoningMinigameUI.SetActive(false);
            if (poundingMinigameUI != null) poundingMinigameUI.SetActive(false);

            switch (state)
            {
                case CookingGameManager.GameState.SelectRecipe:
                    selectRecipePanel.SetActive(true);
                    break;
                case CookingGameManager.GameState.PreStepIntro:
                    preStepIntroPanel.SetActive(true);
                    SetupIntroScreen();
                    break;
                case CookingGameManager.GameState.PlayingMinigame:
                    gameplayHUDPanel.SetActive(true);
                    ActivateMinigameHUDContainer();
                    break;
                case CookingGameManager.GameState.StepResult:
                    stepResultPanel.SetActive(true);
                    break;
                case CookingGameManager.GameState.RecipeComplete:
                    recipeCompletePanel.SetActive(true);
                    break;
            }
        }

        private void SetupIntroScreen()
        {
            if (gameManager == null || gameManager.currentRecipe == null) return;
            
            RecipeStep step = gameManager.currentRecipe.steps[gameManager.currentStepIndex];
            if (upcomingStepInstructionText != null)
            {
                upcomingStepInstructionText.text = $"ขั้นตอนที่ {gameManager.currentStepIndex + 1}: {step.instructionThai}";
            }
            if (upcomingStepIngredientText != null)
            {
                upcomingStepIngredientText.text = $"วัตถุดิบ: {step.targetIngredientName} ({step.minigameType})";
            }
            if (countdownText != null)
            {
                countdownText.text = "เตรียมตัว...";
            }
        }

        public void UpdateIntroCountdown(int val)
        {
            if (countdownText == null) return;

            if (val > 0)
            {
                countdownText.text = val.ToString();
            }
            else
            {
                countdownText.text = "เริ่มปรุง!";
            }
        }

        public void SetupGameplayHUD(RecipeStep step, int currentStepNum, int totalSteps)
        {
            if (stepInstructionText != null)
            {
                stepInstructionText.text = step.instructionThai;
            }
            if (stepProgressText != null)
            {
                stepProgressText.text = $"ขั้นตอน: {currentStepNum} / {totalSteps}";
            }
        }

        private void ActivateMinigameHUDContainer()
        {
            if (gameManager == null || gameManager.currentRecipe == null) return;

            RecipeStep step = gameManager.currentRecipe.steps[gameManager.currentStepIndex];
            switch (step.minigameType)
            {
                case MinigameType.Chopping:
                    if (choppingMinigameUI != null) choppingMinigameUI.SetActive(true);
                    break;
                case MinigameType.Stirring:
                    if (stirringMinigameUI != null) stirringMinigameUI.SetActive(true);
                    break;
                case MinigameType.Grilling:
                    if (grillingMinigameUI != null) grillingMinigameUI.SetActive(true);
                    break;
                case MinigameType.Seasoning:
                    if (seasoningMinigameUI != null) seasoningMinigameUI.SetActive(true);
                    break;
                case MinigameType.Pounding:
                    if (poundingMinigameUI != null) poundingMinigameUI.SetActive(true);
                    break;
            }
        }

        public void ShowFeedback(string text)
        {
            if (feedbackText == null) return;

            feedbackText.text = text;
            
            // Set color based on performance
            if (text.Contains("Perfect")) feedbackText.color = Color.green;
            else if (text.Contains("Good")) feedbackText.color = Color.cyan;
            else if (text.Contains("Too Fast") || text.Contains("Too Slow") || text.Contains("Oops")) feedbackText.color = Color.red;
            else feedbackText.color = Color.yellow; // Under/Over-seasoned warnings

            // Position feedback text in screen center
            feedbackText.transform.localPosition = new Vector3(0f, 100f, 0f);
            
            // Reset fading alpha
            Color c = feedbackText.color;
            c.a = 1f;
            feedbackText.color = c;

            feedbackText.gameObject.SetActive(true);
            feedbackTimer = 1.2f; // Keep visible for 1.2s
        }

        public void ShowStepResult(float score, string stepName)
        {
            if (stepResultTitleText != null)
            {
                stepResultTitleText.text = $"สำเร็จขั้นตอน: {stepName}";
            }

            if (stepResultScoreText != null)
            {
                stepResultScoreText.text = $"{score:F0}%";
            }
        }

        public void ShowFinalRecipeResult(Recipe recipe, float score, string medal)
        {
            if (finalRecipeNameText != null) finalRecipeNameText.text = recipe.recipeNameThai;
            if (finalScoreText != null) finalScoreText.text = $"คะเเนนรวม: {score:F1}%";
            if (finalDescriptionText != null) finalDescriptionText.text = recipe.description;

            if (finalMedalRatingText != null)
            {
                if (medal == "Gold")
                {
                    finalMedalRatingText.text = "เชฟทองคำระดับสากล! (Gold Mama Medal)";
                    finalMedalRatingText.color = goldMedalColor;
                }
                else if (medal == "Silver")
                {
                    finalMedalRatingText.text = "เชฟครัวไทยหัวใจสุขภาพ! (Silver Mama Medal)";
                    finalMedalRatingText.color = silverMedalColor;
                }
                else if (medal == "Bronze")
                {
                    finalMedalRatingText.text = "ผู้ช่วยพ่อครัวเเสนขยัน! (Bronze Mama Medal)";
                    finalMedalRatingText.color = bronzeMedalColor;
                }
                else
                {
                    finalMedalRatingText.text = "พยายามอีกนิด พึ่งเริ่มหัดปรุง! (Try Again)";
                    finalMedalRatingText.color = failColor;
                }
            }

            // Set final medal image color representation
            if (finalMedalImage != null)
            {
                if (medal == "Gold") finalMedalImage.color = goldMedalColor;
                else if (medal == "Silver") finalMedalImage.color = silverMedalColor;
                else if (medal == "Bronze") finalMedalImage.color = bronzeMedalColor;
                else finalMedalImage.color = failColor;
            }
        }

        public void OnNextStepButtonPressed()
        {
            if (gameManager != null)
            {
                gameManager.LoadNextStep();
            }
        }

        public void OnRestartButtonPressed()
        {
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }

        private CookingMinigameBase GetActiveMinigameScript()
        {
            if (gameManager == null) return null;

            if (gameManager.choppingController != null && gameManager.choppingController.gameObject.activeSelf) return gameManager.choppingController;
            if (gameManager.stirringController != null && gameManager.stirringController.gameObject.activeSelf) return gameManager.stirringController;
            
            GrillingMinigame griller = gameManager.grillingControllerDirect != null ? gameManager.grillingControllerDirect : (gameManager.grillingController != null ? gameManager.grillingController.minigame : null);
            if (griller != null && griller.gameObject.activeSelf) return griller;
            
            if (gameManager.seasoningController != null && gameManager.seasoningController.gameObject.activeSelf) return gameManager.seasoningController;
            if (gameManager.poundingController != null && gameManager.poundingController.gameObject.activeSelf) return gameManager.poundingController;

            return null;
        }
    }
}
