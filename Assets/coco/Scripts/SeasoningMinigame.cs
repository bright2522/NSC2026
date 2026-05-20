using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CookingGame
{
    public class SeasoningMinigame : CookingMinigameBase
    {
        [Header("Seasoning Data")]
        [Tooltip("The names of seasonings to pour in sequence (e.g. มะนาว, น้ำปลา Low-Sodium, พริก)")]
        public List<string> seasoningsToPour = new List<string>();
        private int currentSeasoningIndex = 0;

        [Header("Seasoning Gauge")]
        public float currentFill = 0f;
        public float fillSpeed = 50f; // Fill units per second
        private bool isPouring = false;

        [Header("Target Zone (0 to 100)")]
        public float idealMin = 65f;
        public float idealMax = 85f;

        [Header("UI References")]
        [Tooltip("Text label showing the active seasoning name")]
        public Text seasoningNameText;
        [Tooltip("Slider representing the seasoning gauge")]
        public Slider gaugeSlider;
        [Tooltip("Visual target zone highlighting")]
        public RectTransform idealZoneVisual;
        [Tooltip("Instruction overlay")]
        public Text promptText;

        [Header("Visual Effects")]
        [Tooltip("Seasoning bottle object that tilts when pouring")]
        public Transform bottleTransform;
        public Vector3 idleRotation = new Vector3(0f, 0f, 0f);
        public Vector3 pourRotation = new Vector3(0f, 0f, -60f);

        [Tooltip("Particle system showing seasoning pouring out")]
        public ParticleSystem pourParticles;

        private List<float> seasoningScores = new List<float>();

        protected override void OnMinigameStart()
        {
            seasoningScores.Clear();
            currentSeasoningIndex = 0;
            isPouring = false;

            // Extract seasonings from step description or set default healthy ones
            PopulateSeasoningsList();

            // Setup UI for the first seasoning
            SetupSeasoningUI();

            // Set up target zone visual
            if (gaugeSlider != null && idealZoneVisual != null)
            {
                float sliderWidth = gaugeSlider.GetComponent<RectTransform>().rect.width;
                float zoneCenter = (idealMin + idealMax) / 200f; // Normalize to 0-1
                float zoneWidth = (idealMax - idealMin) / 100f;

                idealZoneVisual.anchoredPosition = new Vector2((zoneCenter - 0.5f) * sliderWidth, 0f);
                idealZoneVisual.sizeDelta = new Vector2(zoneWidth * sliderWidth, idealZoneVisual.sizeDelta.y);
            }

            if (pourParticles != null) pourParticles.Stop();
        }

        private void PopulateSeasoningsList()
        {
            seasoningsToPour = new List<string>();

            // Determine seasoning based on active recipe step
            string recipeName = gameManager != null && gameManager.currentRecipe != null ? gameManager.currentRecipe.recipeNameThai : "";
            
            if (recipeName == "ต้มยำน้ำใสปลา + เห็ดรวม")
            {
                seasoningsToPour.Add("มะนาว Low-Sodium (เพิ่มความเปรี้ยว)");
                seasoningsToPour.Add("น้ำปลา Low-Sodium (เพิ่มความเค็มกลมกล่อม)");
                seasoningsToPour.Add("พริกขี้หนูสวน (เพิ่มความเผ็ดร้อน)");
            }
            else if (recipeName == "ยำอกไก่แอปเปิ้ลเขียว")
            {
                seasoningsToPour.Add("มะนาว (เปรี้ยวสดชื่น)");
                seasoningsToPour.Add("น้ำปลา Low-Sodium (กลมกล่อม)");
                seasoningsToPour.Add("พริกขี้หนู (เผ็ดจัดจ้าน)");
            }
            else if (recipeName == "สุกี้น้ำไก่ผักเยอะ")
            {
                seasoningsToPour.Add("น้ำจิ้มสุกี้สุขภาพ (ลดโซเดียม)");
            }
            else if (recipeName == "ข้าวกล้อง + ผัดบล็อกโคลี่เห็ดหอม")
            {
                seasoningsToPour.Add("ซอสปรุงรส Low-Sodium (ความเค็มพอเหมาะ)");
            }
            else if (recipeName == "ลาบปลาไม่ใส่น้ำตาล")
            {
                seasoningsToPour.Add("มะนาว (เปรี้ยวธรรมชาติ)");
                seasoningsToPour.Add("น้ำปลา Low-Sodium (เค็มพอดี)");
                seasoningsToPour.Add("พริกป่น (เผ็ดร้อนสมุนไพร)");
                seasoningsToPour.Add("ข้าวคั่ว (เพิ่มความหอม)");
            }
            else
            {
                // Default fallback
                seasoningsToPour.Add("เครื่องปรุงรสโซเดียมต่ำ");
            }
        }

        private void SetupSeasoningUI()
        {
            if (currentSeasoningIndex >= seasoningsToPour.Count) return;

            currentFill = 0f;
            isPouring = false;

            if (gaugeSlider != null) gaugeSlider.value = 0f;

            if (seasoningNameText != null)
            {
                seasoningNameText.text = seasoningsToPour[currentSeasoningIndex];
            }

            if (promptText != null)
            {
                promptText.text = "กดค้างเพื่อเทเครื่องปรุงเเละปล่อยในเเถบสีเขียว!";
            }

            if (bottleTransform != null)
            {
                bottleTransform.localRotation = Quaternion.Euler(idleRotation);
            }

            if (pourParticles != null) pourParticles.Stop();
        }

        protected override void OnMinigameUpdate()
        {
            if (currentSeasoningIndex >= seasoningsToPour.Count) return;

            // 1. Detect click/hold down to pour
            if (Input.GetMouseButtonDown(0))
            {
                isPouring = true;
                if (pourParticles != null) pourParticles.Play();
            }

            if (Input.GetMouseButtonUp(0) && isPouring)
            {
                isPouring = false;
                if (pourParticles != null) pourParticles.Stop();
                EvaluateSeasoningPour();
            }

            // 2. Adjust bottle rotation & fill gauge
            if (isPouring)
            {
                currentFill += fillSpeed * Time.deltaTime;
                currentFill = Mathf.Clamp(currentFill, 0f, 100f);

                if (gaugeSlider != null)
                {
                    gaugeSlider.value = currentFill / 100f;
                }

                if (bottleTransform != null)
                {
                    bottleTransform.localRotation = Quaternion.Lerp(
                        bottleTransform.localRotation, 
                        Quaternion.Euler(pourRotation), 
                        Time.deltaTime * 8f
                    );
                }

                // If player overfills completely, force evaluate
                if (currentFill >= 100f)
                {
                    isPouring = false;
                    if (pourParticles != null) pourParticles.Stop();
                    EvaluateSeasoningPour();
                }
            }
            else
            {
                if (bottleTransform != null)
                {
                    bottleTransform.localRotation = Quaternion.Lerp(
                        bottleTransform.localRotation, 
                        Quaternion.Euler(idleRotation), 
                        Time.deltaTime * 5f
                    );
                }
            }
        }

        private void EvaluateSeasoningPour()
        {
            float score = 0f;
            string feedback = "Oops!";

            if (currentFill < idealMin)
            {
                // Under-seasoned
                score = Mathf.Lerp(20f, 75f, currentFill / idealMin);
                feedback = "จืดไปหน่อย (Under-seasoned)";
            }
            else if (currentFill <= idealMax)
            {
                // Perfect seasoning!
                score = 100f;
                feedback = "Perfect!";
            }
            else
            {
                // Over-seasoned / Too salty/sour
                float overT = (currentFill - idealMax) / (100f - idealMax);
                score = Mathf.Lerp(75f, 10f, overT);
                feedback = "ปรุงรสเข้มข้นเกินไป! (Over-seasoned)";
            }

            seasoningScores.Add(score);

            if (gameManager != null && gameManager.uiController != null)
            {
                gameManager.uiController.ShowFeedback(feedback);
            }

            Debug.Log($"[Seasoning] Seasoning {currentSeasoningIndex + 1}/{seasoningsToPour.Count} Poured: {currentFill}% -> Score: {score} ({feedback})");

            currentSeasoningIndex++;

            if (currentSeasoningIndex >= seasoningsToPour.Count)
            {
                // End minigame and return average score
                float averageScore = 0f;
                foreach (float s in seasoningScores) averageScore += s;
                averageScore /= seasoningsToPour.Count;

                EndMinigame(averageScore >= 50f, averageScore);
            }
            else
            {
                // Proceed to next seasoning after a short delay
                Invoke(nameof(SetupSeasoningUI), 1f);
            }
        }
    }
}
