using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    public int currentScore = 0;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;

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
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        SaveScore();
        UpdateScoreUI();
        Debug.Log($"<color=green>บวกคะแนน: {amount} | รวม: {currentScore}</color>");
    }

    public void SaveScore()
    {
        PlayerPrefs.SetInt("PlayerScore", currentScore);
        PlayerPrefs.Save();
    }

    public void ResetScore()
    {
        currentScore = 0;
        PlayerPrefs.DeleteKey("PlayerScore");
        UpdateScoreUI();
        Debug.Log("<color=yellow>ล้างคะแนนเรียบร้อย เริ่มต้นที่ 0</color>");
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }
}