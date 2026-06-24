using UnityEngine;
using UnityEngine.UI; 

public class StirFryManager : MonoBehaviour
{
    public static StirFryManager Instance { get; private set; }

    [Header("UI Elements")]
    public Slider stirFrySlider;
    // 🔥 [ช่องใหม่] ลากวัตถุปุ่มวางตะหลิวมาใส่ที่นี่เพื่อให้ระบบเปิดขึ้นมาตอนผัดเสร็จ
    [Tooltip("ลาก GameObject ของปุ่มวางตะหลิว (PutDownButton) มาใส่ช่องนี้")]
    public GameObject putDownButtonObject; 

    [Header("Stir Fry Settings")]
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
        if (stirFrySlider != null)
        {
            stirFrySlider.minValue = 0f;
            stirFrySlider.maxValue = maxProgress;
            stirFrySlider.value = 0f;
        }
    }

    public void IncreaseProgress(float amount)
    {
        if (isCookingFinished) return;

        currentProgress += amount;
        currentProgress = Mathf.Clamp(currentProgress, 0f, maxProgress);

        if (stirFrySlider != null)
        {
            stirFrySlider.value = currentProgress;
        }

        if (currentProgress >= maxProgress && !isCookingFinished)
        {
            TriggerCookingSuccess();
        }
    }

    public void AddProgress(float value)
    {
        IncreaseProgress(value);
    }

    void TriggerCookingSuccess()
    {
        isCookingFinished = true;
        Debug.Log("🍳✨ [SUCCESS] ผัดกระเทียมครบ 100% แล้ว!");

        // 🔥 [จุดสำคัญ] เมื่อผัดเสร็จครบ 100% สั่งให้ปุ่มวางตะหลิวเด้งขึ้นมาบนจอทันที!
        if (putDownButtonObject != null)
        {
            putDownButtonObject.SetActive(true);
        }
    }

    public void ResetProgress()
    {
        currentProgress = 0f;
        isCookingFinished = false;
        if (stirFrySlider != null)
        {
            stirFrySlider.value = 0f;
        }
    }
}