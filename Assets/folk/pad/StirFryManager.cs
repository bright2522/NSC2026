using UnityEngine;
using UnityEngine.UI; // จำเป็นต้องใช้เพื่อคุม Slider UI

public class StirFryManager : MonoBehaviour
{
    public static StirFryManager Instance; // ทำเป็น Singleton เพื่อให้กระเทียมเรียกใช้ง่ายๆ

    [Header("UI Elements")]
    public Slider progressSlider; // ลากหลอด Slider มาใส่ช่องนี้

    [Header("Stir Fry Settings")]
    public float maxProgress = 100f; // แต้มสูงสุดที่หลอดจะเต็ม
    private float currentProgress = 0f;

    void Awake()
    {
        // ตั้งค่าตัวจัดการศูนย์กลาง
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // รีเซ็ตค่าหลอดเริ่มต้นเป็น 0
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = maxProgress;
            progressSlider.value = 0f;
        }
    }

    // ฟังก์ชันรับแต้มที่ถูกเรียกมาจากตัวกระเทียมตอนโดนตะหลิวเขี่ย
    public void AddProgress(float amount)
    {
        if (currentProgress >= maxProgress) return; // ถ้าเต็มแล้วไม่ต้องทำอะไรต่อ

        currentProgress += amount;
        
        // อัปเดตตัวเลขแสดงผลบนหลอด UI
        if (progressSlider != null)
        {
            progressSlider.value = currentProgress;
        }

        Debug.Log($"🍳 [ผัดกระเทียม] ความคืบหน้า: {currentProgress} / {maxProgress}");

        // เช็คเงื่อนไขถ้าหลอดเต็ม (กระเทียมสุกหอมได้ที่แล้ว)
        if (currentProgress >= maxProgress)
        {
            GarlicSuckSuccess();
        }
    }

    void GarlicSuckSuccess()
    {
        Debug.Log("✨ [SUCCESS] กระเทียมเจียวหอมฟุ้งได้ที่แล้ว! สเต็ปต่อไปตอกไข่ใส่ลงไปได้เลย!");
        // ท่านสามารถเขียนสั่งเปิด UI ชนะ หรือเปลี่ยนเข้าสู่โหมดตอกไข่ตรงนี้ได้เลยครับ
    }
}