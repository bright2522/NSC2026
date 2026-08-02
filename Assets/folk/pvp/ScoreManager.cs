using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    public int currentScore = 0;
    public int opponentScore = 0; // 🎯 เพิ่ม: คะแนนของคู่แข่ง

    [Header("UI References")]
    public TextMeshProUGUI scoreText;          // UI คะแนนเรา
    public TextMeshProUGUI opponentScoreText;  // 🎯 เพิ่ม: UI คะแนนคู่แข่ง

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

    // 🎯 เพิ่ม: ฟังก์ชันอัปเดตคะแนนคู่แข่ง
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
        PlayerPrefs.DeleteKey("OpponentScore");
        UpdateScoreUI();
        UpdateOpponentScoreUI();
        Debug.Log("<color=yellow>ล้างคะแนนเรียบร้อย เริ่มต้นที่ 0</color>");
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }

    // 🎯 เพิ่ม: อัปเดตข้อความคะแนนคู่แข่ง
    public void UpdateOpponentScoreUI()
    {
        if (opponentScoreText != null)
        {
            opponentScoreText.text = "Enemy Score: " + opponentScore.ToString();
        }
    }
}