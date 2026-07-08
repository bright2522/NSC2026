using System;
using UnityEngine;

public class GameplayScore : MonoBehaviour
{
    public static GameplayScore Instance { get; private set; }

    public int CurrentScore { get; private set; }
    public event Action<int> OnScoreChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void AddScore(int amount)
    {
        if (amount == 0) return;
        CurrentScore = Mathf.Max(0, CurrentScore + amount);
        OnScoreChanged?.Invoke(CurrentScore);
    }
}
