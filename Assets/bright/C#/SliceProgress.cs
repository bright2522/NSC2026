using UnityEngine;

// นับจำนวนชิ้นที่หั่น (รวมทุก object) พอครบเป้าหมายก็โชว์ปุ่ม Next
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

    [Header("ปุ่ม Next")]
    public GameObject nextButton;

    [Header("แสดงความคืบหน้า (จะใส่หรือไม่ก็ได้)")]
    public TMPro.TMP_Text progressLabel;

    private int count;
    private bool completionBonusAwarded;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ResetProgress(); // เริ่มต้น = รีเซ็ตตัวนับ + ซ่อนปุ่ม
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
        if (nextButton != null) nextButton.SetActive(false);
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
                GameplayScore.Instance?.AddScore(50);
            }

            if (nextButton != null)
                nextButton.SetActive(true);
        }
    }

    void UpdateLabel()
    {
        if (progressLabel != null)
            progressLabel.text = $"{Mathf.Min(count, targetPieces)} / {targetPieces}";
    }

    public int CurrentCount => count;
}