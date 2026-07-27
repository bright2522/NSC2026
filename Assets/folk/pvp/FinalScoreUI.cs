using UnityEngine;
using TMPro; // หากใช้ TextMeshPro
// using UnityEngine.UI; // ให้เปิดบรรทัดนี้แทนหากใช้ UI Text ปกติ

public class FinalScoreUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI scoreText; // หรือ public Text scoreText;

    [Header("Text Format")]
    public string prefixText = "Score: ";

    void Start()
    {
        DisplayFinalScore();
    }

    public void DisplayFinalScore()
    {
        // 📥 ดึงคะแนนที่บันทึกไว้ในคีย์ "PlayerScore" (ถ้าไม่มีข้อมูลจะแสดงเป็น 0)
        int savedScore = PlayerPrefs.GetInt("PlayerScore", 0);

        if (scoreText != null)
        {
            scoreText.text = prefixText + savedScore.ToString();
        }
    }
}