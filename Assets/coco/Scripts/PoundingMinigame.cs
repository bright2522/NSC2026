using UnityEngine;
using UnityEngine.UI;

namespace CookingGame
{
    public class PoundingMinigame : CookingMinigameBase
    {
        [Header("Pounding Settings")]
        public float mashProgress = 0f;
        public float perfectProgressGain = 15f;
        public float goodProgressGain = 10f;
        public float missProgressPenalize = 2f;

        [Header("Rhythm Rings")]
        public RectTransform targetRing;
        public RectTransform shrinkingRing;
        public float shrinkSpeed = 1.5f;
        private float ringScale = 3f;
        private float targetScale = 1f;

        [Header("Pestle Visual Animation")]
        [Tooltip("The pestle transform that will move up/down")]
        public Transform pestleTransform;
        private Vector3 initialPestlePos;
        public float poundDepth = -1.2f;
        private float poundAnimTimer = 0f;

        [Header("UI Slider")]
        public Slider progressSlider;
        public Text promptText;

        protected override void OnMinigameStart()
        {
            mashProgress = 0f;
            ringScale = 3f;
            poundAnimTimer = 0f;

            if (progressSlider != null) progressSlider.value = 0f;
            if (shrinkingRing != null) shrinkingRing.localScale = Vector3.one * ringScale;

            if (pestleTransform != null)
            {
                initialPestlePos = pestleTransform.localPosition;
            }

            if (promptText != null)
            {
                promptText.text = "เคาะจังหวะตอนวงกลมซ้อนทับกันพอดีเพื่อโขลกเครื่องเเกง!";
            }

            Debug.Log($"[Pounding Minigame] Started! Pounding: {currentStep.targetIngredientName}");
        }

        protected override void OnMinigameUpdate()
        {
            // 1. Shrink the ring
            ringScale -= Time.deltaTime * shrinkSpeed;
            
            // If completely missed (shrunk too small)
            if (ringScale <= 0.4f)
            {
                TriggerMiss();
            }

            if (shrinkingRing != null)
            {
                shrinkingRing.localScale = Vector3.one * ringScale;
            }

            // 2. Click to pound
            if (Input.GetMouseButtonDown(0))
            {
                TriggerPound();
            }

            // 3. Pestle animation interpolation
            if (pestleTransform != null && poundAnimTimer > 0f)
            {
                poundAnimTimer -= Time.deltaTime * 6f;
                if (poundAnimTimer <= 0f)
                {
                    pestleTransform.localPosition = initialPestlePos;
                }
                else
                {
                    // Bounce pestle down then back up
                    float heightOffset = Mathf.Sin(poundAnimTimer * Mathf.PI) * poundDepth;
                    pestleTransform.localPosition = initialPestlePos + Vector3.up * heightOffset;
                }
            }
        }

        private void TriggerPound()
        {
            // Activate visual pound animation
            poundAnimTimer = 1f;

            // Evaluate timing
            float diff = Mathf.Abs(ringScale - targetScale);
            float scoreGain = 0f;
            float progressGain = 0f;
            string feedback = "Oops!";

            if (diff <= 0.15f)
            {
                // Perfect timing
                scoreGain = 100f / (100f / perfectProgressGain);
                progressGain = perfectProgressGain;
                feedback = "Perfect!";
            }
            else if (diff <= 0.4f)
            {
                // Good timing
                scoreGain = 70f / (100f / goodProgressGain);
                progressGain = goodProgressGain;
                feedback = "Good!";
            }
            else
            {
                // Off beat
                progressGain = 0f;
                feedback = "Oops!";
            }

            // Apply progress
            mashProgress += progressGain;
            stepScore += scoreGain;

            mashProgress = Mathf.Clamp(mashProgress, 0f, 100f);
            if (progressSlider != null)
            {
                progressSlider.value = mashProgress / 100f;
            }

            if (gameManager != null && gameManager.uiController != null)
            {
                gameManager.uiController.ShowFeedback(feedback);
            }

            Debug.Log($"[Pounding] Pounded! Scale diff: {diff:F2} -> {feedback}. Progress: {mashProgress}%");

            // Reset ring for next beat
            ringScale = 3f;

            // Check completion
            if (mashProgress >= 100f)
            {
                float finalScore = Mathf.Min(stepScore, 100f);
                EndMinigame(finalScore >= 50f, finalScore);
            }
        }

        private void TriggerMiss()
        {
            // Reset ring
            ringScale = 3f;
            
            // Deduct progress lightly for missing beat
            mashProgress -= missProgressPenalize;
            mashProgress = Mathf.Max(mashProgress, 0f);
            
            if (progressSlider != null)
            {
                progressSlider.value = mashProgress / 100f;
            }

            if (gameManager != null && gameManager.uiController != null)
            {
                gameManager.uiController.ShowFeedback("Oops! Miss");
            }
        }

        protected override void OnMinigameEnd()
        {
            if (pestleTransform != null)
            {
                pestleTransform.localPosition = initialPestlePos;
            }
        }
    }
}
