using UnityEngine;
using UnityEngine.UI;

namespace CookingGame
{
    public class GrillingMinigame : CookingMinigameBase
    {
        public enum GrillState
        {
            CookingSideA,
            CookingSideB,
            Completed
        }

        [Header("Grilling Settings")]
        public GrillState currentState;
        public float cookProgress = 0f;
        public float cookSpeed = 12f; // Progress units per second

        [Header("Ideal Cooking Window")]
        public float idealMin = 50f;
        public float idealMax = 80f;

        [Header("UI References")]
        [Tooltip("Slider displaying cooking progress for the current side")]
        public Slider cookProgressSlider;
        [Tooltip("Image overlay showing progress color (Raw, Cooked, Burnt)")]
        public Image progressFillImage;
        [Tooltip("Visual indicator of the ideal zone on the slider")]
        public RectTransform idealZoneVisual;

        [Header("Colors")]
        public Color rawColor = Color.red;
        public Color cookedColor = Color.green;
        public Color burntColor = Color.black;

        [Header("Ingredient Visuals")]
        [Tooltip("The Renderer of the food model to fade color (Raw -> Cooked -> Burnt)")]
        public Renderer foodRenderer;
        public Color foodRawColor = new Color(0.9f, 0.6f, 0.6f); // Raw pinkish
        public Color foodCookedColor = new Color(0.7f, 0.4f, 0.2f); // Cooked brown
        public Color foodBurntColor = new Color(0.15f, 0.15f, 0.15f); // Charcoal black

        private float sideAScore = 0f;
        private float sideBScore = 0f;

        protected override void OnMinigameStart()
        {
            currentState = GrillState.CookingSideA;
            cookProgress = 0f;
            sideAScore = 0f;
            sideBScore = 0f;

            if (cookProgressSlider != null)
            {
                cookProgressSlider.value = 0f;
            }

            // Set up ideal zone visual position
            if (cookProgressSlider != null && idealZoneVisual != null)
            {
                float sliderWidth = cookProgressSlider.GetComponent<RectTransform>().rect.width;
                float zoneCenter = (idealMin + idealMax) / 200f; // Scale to 0-1
                float zoneWidth = (idealMax - idealMin) / 100f;

                idealZoneVisual.anchoredPosition = new Vector2((zoneCenter - 0.5f) * sliderWidth, 0f);
                idealZoneVisual.sizeDelta = new Vector2(zoneWidth * sliderWidth, idealZoneVisual.sizeDelta.y);
            }

            UpdateFoodColor();
            Debug.Log($"[Grilling Minigame] Started! Grilling: {currentStep.targetIngredientName}");
        }

        protected override void OnMinigameUpdate()
        {
            if (currentState == GrillState.Completed) return;

            // 1. Advance cook progress
            cookProgress += cookSpeed * Time.deltaTime;
            cookProgress = Mathf.Clamp(cookProgress, 0f, 100f);

            // Update UI Slider
            if (cookProgressSlider != null)
            {
                cookProgressSlider.value = cookProgress / 100f;
            }

            // Update progress bar fill color based on state
            UpdateProgressUIColors();

            // Update 3D Food model color to show cooking progress
            UpdateFoodColor();

            // 2. Handle Tap to Flip/Plate
            if (Input.GetMouseButtonDown(0))
            {
                HandleGrillAction();
            }
        }

        private void UpdateProgressUIColors()
        {
            if (progressFillImage == null) return;

            if (cookProgress < idealMin)
            {
                // Raw
                progressFillImage.color = Color.Lerp(rawColor, Color.yellow, cookProgress / idealMin);
            }
            else if (cookProgress <= idealMax)
            {
                // Cooked
                progressFillImage.color = cookedColor;
            }
            else
            {
                // Burning
                float burntT = (cookProgress - idealMax) / (100f - idealMax);
                progressFillImage.color = Color.Lerp(cookedColor, burntColor, burntT);
            }
        }

        private void UpdateFoodColor()
        {
            if (foodRenderer == null) return;

            Color targetFoodColor;
            if (cookProgress < idealMin)
            {
                targetFoodColor = Color.Lerp(foodRawColor, foodCookedColor, cookProgress / idealMin);
            }
            else if (cookProgress <= idealMax)
            {
                targetFoodColor = foodCookedColor;
            }
            else
            {
                float burntT = (cookProgress - idealMax) / (100f - idealMax);
                targetFoodColor = Color.Lerp(foodCookedColor, foodBurntColor, burntT);
            }

            // Set the material color
            foodRenderer.material.color = targetFoodColor;
        }

        private void HandleGrillAction()
        {
            float currentScore = CalculateActionScore();
            string feedback = GetPerformanceRating(currentScore);

            if (gameManager != null && gameManager.uiController != null)
            {
                gameManager.uiController.ShowFeedback(feedback);
            }

            if (currentState == GrillState.CookingSideA)
            {
                sideAScore = currentScore;
                Debug.Log($"[Grilling] Side A Flipped! Score: {sideAScore} ({feedback})");

                // Flip to Side B
                currentState = GrillState.CookingSideB;
                cookProgress = 0f; // Reset progress for Side B
                
                // Optional: Play flip animation (rotate model 180 degrees)
                if (foodRenderer != null)
                {
                    foodRenderer.transform.Rotate(Vector3.forward, 180f);
                }
            }
            else if (currentState == GrillState.CookingSideB)
            {
                sideBScore = currentScore;
                Debug.Log($"[Grilling] Side B Plated! Score: {sideBScore} ({feedback})");

                currentState = GrillState.Completed;
                
                // End minigame
                float finalScore = (sideAScore + sideBScore) / 2f;
                EndMinigame(finalScore >= 50f, finalScore);
            }
        }

        private float CalculateActionScore()
        {
            if (cookProgress < idealMin)
            {
                // Underdone
                return Mathf.Lerp(10f, 60f, cookProgress / idealMin);
            }
            else if (cookProgress <= idealMax)
            {
                // Perfectly cooked - check how close to center
                float idealCenter = (idealMin + idealMax) / 2f;
                float diff = Mathf.Abs(cookProgress - idealCenter);
                float maxDiff = (idealMax - idealMin) / 2f;
                return Mathf.Lerp(100f, 80f, diff / maxDiff);
            }
            else
            {
                // Burnt
                float burntT = (cookProgress - idealMax) / (100f - idealMax);
                return Mathf.Lerp(60f, 0f, burntT);
            }
        }
    }
}
