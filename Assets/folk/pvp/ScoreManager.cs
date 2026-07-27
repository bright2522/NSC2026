using UnityEngine;
using TMPro; // สำหรับ Unity 6 ที่ใช้ TextMeshPro เป็นหลัก

public class ScoreManager : MonoBehaviour
{
    // สร้างเป็น Singleton เพื่อให้สคริปต์อื่นเรียกใช้ได้ง่ายๆ โดยไม่ต้อง Link ใน Inspector
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    public int currentScore = 0; // คะแนนเริ่มต้นคือ 0

    [Header("UI References")]
    public TextMeshProUGUI scoreText; // ลาก TextMeshProUGUI มาใส่ตรงนี้

    private void Awake()
    {
        // จัดการ Singleton ป้องกันการมี ScoreManager ซ้ำซ้อน
        if (Instance == null)
        {
            Instance = this;
            // ป้องกันไม่ให้ ScoreManager ถูกลบเมื่อมีการเปลี่ยนสเตชัน
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // ถ้ามีตัวเดิมค้างมาจากซีนก่อน ให้ลบตัวใหม่นี้ทิ้ง
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 🔄 รีเซ็ตคะแนนเป็น 0 และล้างค่าเก่าใน PlayerPrefs ทุกครั้งที่เริ่มเล่นใหม่
        ResetScore();
    }

    // ฟังก์ชันสำหรับเพิ่มคะแนน
    public void AddScore(int amount)
    {
        currentScore += amount;
        
        // 💾 บันทึกคะแนนลงเครื่องทันทีที่มีการบวกคะแนน
        SaveScore();
        
        UpdateScoreUI();
        Debug.Log($"<color=green>บวกคะแนน: {amount} | รวม: {currentScore}</color>");
    }

    // 💾 บันทึกคะแนนลง PlayerPrefs (ใช้คีย์ชื่อ "PlayerScore")
    public void SaveScore()
    {
        PlayerPrefs.SetInt("PlayerScore", currentScore);
        PlayerPrefs.Save();
    }

    // 🔄 ฟังก์ชันล้างคะแนน (เริ่มต้นรอบใหม่เป็น 0)
    public void ResetScore()
    {
        currentScore = 0;
        PlayerPrefs.DeleteKey("PlayerScore"); // ลบค่าคะแนนเก่าที่เซฟไว้ออก
        UpdateScoreUI();
        Debug.Log("<color=yellow>ล้างคะแนนเรียบร้อย เริ่มต้นที่ 0</color>");
    }

    // อัปเดตข้อความบนหน้าจอ
    public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }
}