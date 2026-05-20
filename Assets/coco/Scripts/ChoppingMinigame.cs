using UnityEngine;
using UnityEngine.UI;

namespace CookingGame
{
    public class ChoppingMinigame : CookingMinigameBase
    {
        [Header("Chopping Settings")]
        [Tooltip("Number of successful chops required to finish the ingredient")]
        public int targetChops = 8;
        public int currentChops = 0;

        [Header("Slider UI References")]
        [Tooltip("Slider used for the moving needle")]
        public Slider cuttingSlider;
        [Tooltip("The visual target area on the slider")]
        public RectTransform targetAreaVisual;
        
        [Header("Slider Speeds")]
        public float sliderSpeed = 2f;
        private float sliderValue = 0f;
        private bool movingForward = true;

        [Header("Target Area Ranges (0 to 1)")]
        public float targetCenter = 0.5f;
        public float targetWidth = 0.15f; // Perfect zone width

        [Header("Visual Elements")]
        [Tooltip("Knife transform that will chop down")]
        public Transform knifeTransform;
        private Vector3 initialKnifePos;
        private float knifeChopOffset = -1f; // Move down on Y axis
        private float chopAnimTimer = 0f;

        [Tooltip("Container holding the ingredient model")]
        public Transform ingredientSpawnPoint;
        [Tooltip("Prefab of the chopped food piece to scatter")]
        public GameObject choppedPiecePrefab; // If assigned, we scatter slices

        protected override void OnMinigameStart()
        {
            currentChops = 0;
            sliderValue = 0f;
            movingForward = true;

            if (knifeTransform != null)
            {
                initialKnifePos = knifeTransform.localPosition;
            }

            // Set up target area visual position
            if (cuttingSlider != null && targetAreaVisual != null)
            {
                cuttingSlider.value = 0f;
                // Position the green indicator visually over the middle of the slider
                float sliderWidth = cuttingSlider.GetComponent<RectTransform>().rect.width;
                targetAreaVisual.anchoredPosition = new Vector2((targetCenter - 0.5f) * sliderWidth, 0f);
                targetAreaVisual.sizeDelta = new Vector2(targetWidth * sliderWidth, targetAreaVisual.sizeDelta.y);
            }

            Debug.Log($"[Chopping Minigame] Started! Target: {targetChops} chops of {currentStep.targetIngredientName}");
        }

        protected override void OnMinigameUpdate()
        {
            // 1. Move the cutting slider needle back and forth
            if (cuttingSlider != null)
            {
                if (movingForward)
                {
                    sliderValue += Time.deltaTime * sliderSpeed;
                    if (sliderValue >= 1f)
                    {
                        sliderValue = 1f;
                        movingForward = false;
                    }
                }
                else
                {
                    sliderValue -= Time.deltaTime * sliderSpeed;
                    if (sliderValue <= 0f)
                    {
                        sliderValue = 0f;
                        movingForward = true;
                    }
                }
                cuttingSlider.value = sliderValue;
            }

            // 2. Handle Player Input (Click / Touch)
            if (Input.GetMouseButtonDown(0))
            {
                TriggerChop();
            }

            // 3. Knife return animation
            if (knifeTransform != null && chopAnimTimer > 0f)
            {
                chopAnimTimer -= Time.deltaTime * 5f;
                if (chopAnimTimer <= 0f)
                {
                    knifeTransform.localPosition = initialKnifePos;
                }
                else
                {
                    knifeTransform.localPosition = Vector3.Lerp(initialKnifePos, initialKnifePos + Vector3.up * knifeChopOffset, chopAnimTimer);
                }
            }
        }

        private void TriggerChop()
        {
            // Knife chop motion
            chopAnimTimer = 1f;

            // Check timing
            float diff = Mathf.Abs(sliderValue - targetCenter);
            float scoreGain = 0f;
            string feedback = "Oops!";

            if (diff <= targetWidth / 2f)
            {
                // Perfect hit!
                scoreGain = 100f / targetChops;
                feedback = "Perfect!";
                currentChops++;
            }
            else if (diff <= targetWidth)
            {
                // Good hit
                scoreGain = 70f / targetChops;
                feedback = "Good!";
                currentChops++;
            }
            else
            {
                // Miss
                scoreGain = 20f / targetChops;
                feedback = "Oops!";
                // In Cooking Mama, even a bad tap moves progress slightly but reduces final rating
                currentChops++; 
            }

            stepScore += scoreGain;

            // Spawn visual chop slices if prefabs are configured
            if (choppedPiecePrefab != null && ingredientSpawnPoint != null)
            {
                GameObject slice = Instantiate(choppedPiecePrefab, ingredientSpawnPoint.position, Quaternion.identity);
                Rigidbody rb = slice.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(new Vector3(Random.Range(-2f, 2f), Random.Range(3f, 6f), Random.Range(-2f, 2f)), ForceMode.Impulse);
                    rb.AddTorque(new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f)), ForceMode.Impulse);
                }
                Destroy(slice, 3f);
            }

            // Report action feedback to UI via Manager
            if (gameManager != null && gameManager.uiController != null)
            {
                gameManager.uiController.ShowFeedback(feedback);
            }

            Debug.Log($"[Chopping] Chop {currentChops}/{targetChops}! Rating: {feedback}, Score +{scoreGain}, Total: {stepScore}");

            if (currentChops >= targetChops)
            {
                // Finished chopping!
                float finalScore = Mathf.Min(stepScore, 100f);
                EndMinigame(finalScore >= 50f, finalScore);
            }
        }

        protected override void OnMinigameEnd()
        {
            if (knifeTransform != null)
            {
                knifeTransform.localPosition = initialKnifePos;
            }
        }
    }
}
