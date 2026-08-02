using UnityEngine;

public class OpponentScoreSync : MonoBehaviour
{
    [Header("Player Role Setup")]
    [Tooltip("กำหนดว่าจอนี้คือ Player 1 หรือ Player 2")]
    public int myPlayerID = 1; 

    private int lastOpponentScore = -1;

    private void Awake()
    {
        // 💡 เทคนิค: ตรวจสอบว่าถ้ามี Player 1 อยู่ในระบบแล้ว ให้เครื่อง/อินสแตนซ์นี้กลายเป็น Player 2 อัตโนมัติ
        if (PlayerPrefs.HasKey("P1_Online") && !PlayerPrefs.HasKey("P2_Online"))
        {
            myPlayerID = 2;
            PlayerPrefs.SetInt("P2_Online", 1);
        }
        else
        {
            myPlayerID = 1;
            PlayerPrefs.SetInt("P1_Online", 1);
        }
    }

    void Update()
    {
        SyncScores();
    }

    void SyncScores()
    {
        if (ScoreManager.Instance == null) return;

        // 1. บันทึกคะแนนของเราลงในช่องของตัวเอง
        string myKey = "P" + myPlayerID + "_Score";
        PlayerPrefs.SetInt(myKey, ScoreManager.Instance.currentScore);

        // 2. อ่านคะแนนของคู่แข่ง (ถ้าเราคือ P1 คู่แข่งคือ P2 / ถ้าเราคือ P2 คู่แข่งคือ P1)
        int opponentID = (myPlayerID == 1) ? 2 : 1;
        string opponentKey = "P" + opponentID + "_Score";

        int currentOpponentScore = PlayerPrefs.GetInt(opponentKey, 0);

        // 3. ถ้าคะแนนคู่แข่งมีการเปลี่ยนแปลง ให้ส่งไปอัปเดต UI
        if (currentOpponentScore != lastOpponentScore)
        {
            lastOpponentScore = currentOpponentScore;
            ScoreManager.Instance.SetOpponentScore(currentOpponentScore);
        }
    }

    private void OnApplicationQuit()
    {
        // ล้างสถานะเมื่อปิดเกม
        PlayerPrefs.DeleteKey("P1_Online");
        PlayerPrefs.DeleteKey("P2_Online");
        PlayerPrefs.DeleteKey("P1_Score");
        PlayerPrefs.DeleteKey("P2_Score");
    }
}