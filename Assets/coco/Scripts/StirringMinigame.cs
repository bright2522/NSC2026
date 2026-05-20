using UnityEngine;
using UnityEngine.UI;

namespace CookingGame
{
    public class StirringMinigame : CookingMinigameBase
    {
        [Header("Stirring Settings")]
        [Tooltip("How fast the cooking progress fills when stirring in the ideal zone")]
        public float cookSpeed = 15f;
        public float cookingProgress = 0f;

        [Header("Stir Speed Meter")]
        public float currentStirSpeed = 0f;
        public float friction = 2.5f; // Speed decay rate
        public float speedGainMultiplier = 15f; // Sensitivity of dragging

        [Header("Target Zone (0 to 1)")]
        public float idealSpeedMin = 0.35f;
        public float idealSpeedMax = 0.7f;

        [Header("UI References")]
        [Tooltip("Displays the current stir speed")]
        public Slider speedSlider;
        [Tooltip("Visual indicator of the ideal speed zone")]
        public RectTransform idealZoneVisual;
        [Tooltip("Displays the cooking progress from 0% to 100%")]
        public Slider progressSlider;

        [Header("References")]
        [Tooltip("The pot/pan transform. Circular movement is tracked around this object's screen position.")]
        public Transform potCenterTransform;
        [Tooltip("The ladle or spoon that follows the mouse/touch")]
        public Transform spoonTransform;

        private Vector2 lastMouseVector;
        private bool isDragging = false;
        private float perfectTimeBonus = 0f;

        protected override void OnMinigameStart()
        {
            cookingProgress = 0f;
            currentStirSpeed = 0f;
            isDragging = false;
            perfectTimeBonus = 0f;

            if (progressSlider != null) progressSlider.value = 0f;
            if (speedSlider != null) speedSlider.value = 0f;

            // Set up ideal zone visual position
            if (speedSlider != null && idealZoneVisual != null)
            {
                float sliderWidth = speedSlider.GetComponent<RectTransform>().rect.width;
                float zoneCenter = (idealSpeedMin + idealSpeedMax) / 2f;
                float zoneWidth = idealSpeedMax - idealSpeedMin;

                idealZoneVisual.anchoredPosition = new Vector2((zoneCenter - 0.5f) * sliderWidth, 0f);
                idealZoneVisual.sizeDelta = new Vector2(zoneWidth * sliderWidth, idealZoneVisual.sizeDelta.y);
            }

            Debug.Log($"[Stirring Minigame] Started! Cooking: {currentStep.targetIngredientName}");
        }

        protected override void OnMinigameUpdate()
        {
            // 1. Detect drag start/end
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastMouseVector = GetMouseVectorFromCenter();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            // 2. Track circular dragging movement
            if (isDragging)
            {
                Vector2 currentMouseVector = GetMouseVectorFromCenter();
                if (currentMouseVector.magnitude > 10f) // Threshold to avoid noise
                {
                    // Calculate angle difference between this frame and last frame
                    float angleDiff = Vector2.SignedAngle(lastMouseVector, currentMouseVector);
                    
                    // Stir speed increases with rotation delta (ignoring direction sign to allow stirring either way)
                    currentStirSpeed += Mathf.Abs(angleDiff) * Time.deltaTime * speedGainMultiplier;
                    lastMouseVector = currentMouseVector;
                }

                // Make the spoon visual follow the mouse/touch
                if (spoonTransform != null)
                {
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mouseWorldPos.z = spoonTransform.position.z; // Maintain original Z depth
                    spoonTransform.position = mouseWorldPos;
                }
            }

            // 3. Apply friction to decay speed over time
            currentStirSpeed -= friction * Time.deltaTime;
            currentStirSpeed = Mathf.Clamp(currentStirSpeed, 0f, 100f);

            // Update UI Speed Slider (normalize to 0-1)
            float normalizedSpeed = currentStirSpeed / 100f;
            if (speedSlider != null)
            {
                speedSlider.value = normalizedSpeed;
            }

            // 4. Update cooking progress based on stir speed zone
            string feedback = "";
            if (normalizedSpeed >= idealSpeedMin && normalizedSpeed <= idealSpeedMax)
            {
                // In ideal zone
                cookingProgress += cookSpeed * Time.deltaTime;
                perfectTimeBonus += Time.deltaTime;
                feedback = "Perfect!";
            }
            else if (normalizedSpeed > idealSpeedMax)
            {
                // Too fast! (Penalize or slow down progress)
                cookingProgress += (cookSpeed * 0.2f) * Time.deltaTime; // Cooks very slowly or burns
                feedback = "Too Fast!";
            }
            else if (normalizedSpeed > 0.05f)
            {
                // Too slow
                cookingProgress += (cookSpeed * 0.4f) * Time.deltaTime;
                feedback = "Too Slow!";
            }

            cookingProgress = Mathf.Clamp(cookingProgress, 0f, 100f);
            if (progressSlider != null)
            {
                progressSlider.value = cookingProgress / 100f;
            }

            // Display floating feedback in the game HUD occasionally
            if (isDragging && Time.frameCount % 30 == 0 && feedback != "" && gameManager != null && gameManager.uiController != null)
            {
                gameManager.uiController.ShowFeedback(feedback);
            }

            // 5. Completion Check
            if (cookingProgress >= 100f)
            {
                // Success! Calculate score based on how long they spent in the perfect zone
                float ratioInPerfect = perfectTimeBonus / (timeLimit - timeRemaining);
                float finalScore = Mathf.Lerp(50f, 100f, ratioInPerfect);
                EndMinigame(true, finalScore);
            }
        }

        private Vector2 GetMouseVectorFromCenter()
        {
            Vector2 centerScreenPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
            
            // If we have a pot transform, project it to screen coordinates
            if (potCenterTransform != null)
            {
                Vector3 screenPos3D = Camera.main.WorldToScreenPoint(potCenterTransform.position);
                centerScreenPos = new Vector2(screenPos3D.x, screenPos3D.y);
            }

            return (Vector2)Input.mousePosition - centerScreenPos;
        }

        protected override void OnMinigameEnd()
        {
            isDragging = false;
        }
    }
}
