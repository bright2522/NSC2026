using UnityEngine;

public class StirFryManager : MonoBehaviour
{
    public static StirFryManager Instance { get; private set; }

    [Header("UI Elements (ซ่อนหลอดเหลือแค่ปุ่มพาสไปต่อ)")]
    [Tooltip("ลาก GameObject ของปุ่มวางตะหลิว (PutDownButton) มาใส่ช่องนี้")]
    public GameObject putDownButtonObject; 

    [Header("Stir Fry Settings (นับแต้มเบื้องหลังเงียบๆ)")]
    public float maxProgress = 100f;
    
    private float currentProgress = 0f;
    private bool isCookingFinished = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // เคลียร์ระบบ UI เก่าออกไปอย่างปลอดภัยแล้ว
    }

    public void IncreaseProgress(float amount)
    {
        if (isCookingFinished) return;

        // สะสมความสุกระบบหลังบ้านเงียบๆ ผู้เล่นไม่เห็นหลอด
        currentProgress += amount;
        currentProgress = Mathf.Clamp(currentProgress, 0f, maxProgress);

        Debug.Log($"[ระบบหลังบ้าน] กำลังผัดวัตถุดิบ... ความสุกภายใน: {currentProgress} / {maxProgress}");

        // เมื่อผัดจนได้ที่ครบ 100%
        if (currentProgress >= maxProgress && !isCookingFinished)
        {
            TriggerCookingSuccess();
        }
    }

    // ฟังก์ชันนี้เก็บไว้เพื่อให้สคริปต์กระเทียมเรียกใช้งานได้สะดวก
    public void AddProgress(float value)
    {
        IncreaseProgress(value);
    }

    void TriggerCookingSuccess()
    {
        isCookingFinished = true;
        Debug.Log("🍳✨ [SUCCESS] ผัดวัตถุดิบจนสุกได้ที่เรียบร้อย!");

        // เมื่อผัดเสร็จครบกำหนด สั่งให้ปุ่มวางตะหลิวเด้งขึ้นมาบนจอทันที!
        if (putDownButtonObject != null)
        {
            putDownButtonObject.SetActive(true);
        }
    }

    public void ResetProgress()
    {
        currentProgress = 0f;
        isCookingFinished = false;
    }
}