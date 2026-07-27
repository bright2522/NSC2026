using UnityEngine;

// นับจำนวนชิ้นที่หั่น (รวมทุก object) พอครบเป้าหมายก็โชว์ปุ่ม/รูปภาพ + ปลดล็อกการเลื่อนหน้าจอ
public class SliceProgress : MonoBehaviour
{
    public static SliceProgress Instance { get; private set; }

    public enum CountMode
    {
        TotalPieces,  // นับจำนวนชิ้นรวม (เริ่มจากจำนวนอาหาร แล้ว +1 ต่อการหั่น)
        TotalSlices   // นับจำนวนครั้งที่หั่นรวมทุก object
    }

    [Header("โหมดการนับ")]
    public CountMode countMode = CountMode.TotalSlices;

    [Header("เป้าหมาย")]
    public int targetPieces = 7;
    public int startingPieces = 1;  // (โหมด TotalPieces) เริ่มมีอาหารกี่ก้อน

    [Header("รูปภาพ / ปุ่มที่จะโชว์เมื่อหั่นครบ")]
    [Tooltip("ลากรูปภาพ UI หรือปุ่ม Next ที่เตรียมไว้มาใส่ตรงนี้")]
    public GameObject nextButton;

    [Header("เชื่อมโยงระบบเลื่อนหน้าจอ")]
    [Tooltip("ลาก GameObject ที่มีสคริปต์ SwipeStationSlider มาใส่ตรงนี้ (หากไม่ใส่ ระบบจะหา Singleton อัตโนมัติ)")]
    public SwipeStationSlider stationSlider;

    [Header("แสดงความคืบหน้า (จะใส่หรือไม่ก็ได้)")]
    public TMPro.TMP_Text progressLabel;

    private int count;
    private bool completionBonusAwarded;

    void Awake()
    {
        // 💡 ปรับปรุง: ย้าย Instance มาชี้ที่ตัวใหม่ล่าสุดเสมอ (ไม่สั่ง Destroy)
        Instance = this;
    }

    void Start()
    {
        // หากไม่ได้ลาก stationSlider ใน Inspector ให้ค้นหาจาก Instance อัตโนมัติ
        if (stationSlider == null)
        {
            stationSlider = SwipeStationSlider.Instance;
        }

        ResetProgress(); // เริ่มต้น = รีเซ็ตตัวนับ + ซ่อนรูป/ปุ่ม + ล็อกหน้าจอ
    }

    // เรียกทุกครั้งที่หั่นสำเร็จ 1 ครั้ง (จาก SliceableFood)
    public void AddSlice()
    {
        count++;
        CheckDone();
    }

    // เรียกตอน spawn อาหารใหม่เข้าฉาก (เฉพาะโหมด TotalPieces)
    public void RegisterFood()
    {
        if (countMode == CountMode.TotalPieces) count++;
        CheckDone();
    }

    // *** เริ่มนับใหม่ — ผูกกับปุ่ม Next (อยู่ในซีนเดิม) ***
    public void ResetProgress()
    {
        count = (countMode == CountMode.TotalPieces) ? startingPieces : 0;
        completionBonusAwarded = false;

        // ซ่อนรูปภาพ/ปุ่ม
        HideNextButton();

        // 🔒 ล็อกการเลื่อนหน้าจอทันทีตอนเริ่ม/รีเซ็ต
        GetStationSlider()?.SetSwipeEnabled(false);

        UpdateLabel();
    }

    void CheckDone()
    {
        UpdateLabel();
        if (count >= targetPieces)
        {
            if (!completionBonusAwarded)
            {
                completionBonusAwarded = true;

                // 🎯 ให้คะแนนโบนัสเมื่อหั่นครบ (รองรับทั้ง GameplayScore และ ScoreManager)
                if (GameplayScore.Instance != null)
                {
                    GameplayScore.Instance.AddScore(50);
                }
                else if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(50);
                }
            }

            // 🖼️ 1. แสดงรูปภาพ / ปุ่ม Next ที่เตรียมไว้
            ShowNextButton();

            // 🔓 2. ปลดล็อกสคริปต์เลื่อนหน้าจอให้เลื่อนได้ 1 ครั้งเมื่อหั่นครบ!
            SwipeStationSlider slider = GetStationSlider();
            if (slider != null)
            {
                slider.EnableOneTimeSwipe();
                Debug.Log("<color=green><b>หั่นครบแล้ว! ปลดล็อกให้ปัดหน้าจอได้ 1 ครั้ง</b></color>");
            }
            else
            {
                Debug.LogWarning("[SliceProgress] หา SwipeStationSlider ไม่เจอ!");
            }
        }
    }

    void UpdateLabel()
    {
        if (progressLabel != null)
            progressLabel.text = $"{Mathf.Min(count, targetPieces)} / {targetPieces}";
    }

    // ฟังก์ชันช่วยดึง SwipeStationSlider ที่ปลอดภัย ป้องกัน null
    private SwipeStationSlider GetStationSlider()
    {
        if (stationSlider == null)
        {
            stationSlider = SwipeStationSlider.Instance;
        }
        return stationSlider;
    }

    // ฟังก์ชันเปิด/ปิด ปุ่ม UI ให้เรียกใช้ได้ง่ายและปลอดภัย
    public void HideNextButton()
    {
        if (nextButton != null)
            nextButton.SetActive(false);
    }

    public void ShowNextButton()
    {
        if (nextButton != null)
            nextButton.SetActive(true);
    }

    public int CurrentCount => count;
}