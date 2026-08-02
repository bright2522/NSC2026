using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    public int currentScore = 0;
    public int opponentScore = 0;

    [Header("UI References")]
    [Tooltip("ลาก Text แสดงคะแนนของเรามาใส่")]
    public TextMeshProUGUI scoreText;
    [Tooltip("ลาก Text แสดงคะแนนคู่แข่งมาใส่")]
    public TextMeshProUGUI opponentScoreText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        UpdateScoreUI();
        UpdateOpponentScoreUI();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        SaveScore();
        UpdateScoreUI();
        Debug.Log($"<color=green>บวกคะแนน: {amount} | รวม: {currentScore}</color>");
    }

    public void SetOpponentScore(int score)
    {
        opponentScore = score;
        UpdateOpponentScoreUI();
    }

    public void SaveScore()
    {
        PlayerPrefs.SetInt("PlayerScore", currentScore);
        PlayerPrefs.Save();
    }

    public void ResetScore()
    {
        currentScore = 0;
        opponentScore = 0;
        PlayerPrefs.DeleteKey("PlayerScore");
        UpdateScoreUI();
        UpdateOpponentScoreUI();
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }

    public void UpdateOpponentScoreUI()
    {
        if (opponentScoreText != null)
        {
            opponentScoreText.text = "Enemy Score: " + opponentScore.ToString();
        }
    }
}