using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pep.Scoring
{
    [Serializable]
    public class StepScoreEntry
    {
        public string source;
        public string stepName;
        public float score;
        public float time;
    }

    public class ScoringManager : MonoBehaviour
    {
        [SerializeField] private float minScore = 0f;
        [SerializeField] private float maxScore = 100f;
        [SerializeField] private bool clearOnAwake = true;

        public event Action<float> OnStepScoreReported;
        public event Action<float> OnTotalScoreChanged;
        public event Action<float> OnRecipeCompleted;

        public float TotalScore { get; private set; }
        public int StepCount => stepScores.Count;
        public float AverageScore => stepScores.Count == 0 ? 0f : TotalScore / stepScores.Count;

        private readonly List<StepScoreEntry> stepScores = new List<StepScoreEntry>();

        private void Awake()
        {
            if (clearOnAwake) ResetScores();
        }

        public void ResetScores()
        {
            stepScores.Clear();
            TotalScore = 0f;
            OnTotalScoreChanged?.Invoke(TotalScore);
        }

        public void ReportStepScore(float score)
        {
            ReportStepScore("Unknown", "Step", score);
        }

        public void ReportStepScore(string source, string stepName, float score)
        {
            float clamped = Mathf.Clamp(score, minScore, maxScore);
            stepScores.Add(new StepScoreEntry
            {
                source = string.IsNullOrWhiteSpace(source) ? "Unknown" : source,
                stepName = string.IsNullOrWhiteSpace(stepName) ? $"Step {stepScores.Count + 1}" : stepName,
                score = clamped,
                time = Time.time
            });

            TotalScore += clamped;
            OnStepScoreReported?.Invoke(clamped);
            OnTotalScoreChanged?.Invoke(TotalScore);
        }

        public void ReportFromCocoStepCompleted(float scoreFromCoco)
        {
            ReportStepScore("coco/CookingGameManager", "OnStepCompleted", scoreFromCoco);
        }

        public void SyncFromCocoTotalRecipeScore(float totalRecipeScore)
        {
            TotalScore = Mathf.Clamp(totalRecipeScore, 0f, maxScore * Mathf.Max(1, StepCount));
            OnTotalScoreChanged?.Invoke(TotalScore);
        }

        public float CompleteRecipeAndGetFinalAverage()
        {
            float result = AverageScore;
            OnRecipeCompleted?.Invoke(result);
            return result;
        }

        public string GetMedal(float score)
        {
            if (score >= 90f) return "Gold";
            if (score >= 70f) return "Silver";
            if (score >= 50f) return "Bronze";
            return "Fail";
        }

        public List<StepScoreEntry> GetStepScoreSnapshot()
        {
            var copy = new List<StepScoreEntry>(stepScores.Count);
            foreach (var entry in stepScores)
            {
                copy.Add(new StepScoreEntry
                {
                    source = entry.source,
                    stepName = entry.stepName,
                    score = entry.score,
                    time = entry.time
                });
            }
            return copy;
        }
    }
}
