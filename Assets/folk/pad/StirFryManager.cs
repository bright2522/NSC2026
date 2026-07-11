using UnityEngine;

public class StirFryManager : MonoBehaviour
{
    public static StirFryManager Instance { get; private set; }

    [Header("UI Elements")]
    [Tooltip("ลาก GameObject ของปุ่มวางตะหลิว (PutDownButton) มาใส่ช่องนี้")]
    public GameObject putDownButtonObject;

    [Header("UI / ปุ่มที่จะเปิดหลังผัดเสร็จ")]
    [Tooltip("ลากปุ่ม Next มาใส่ — แบบเดียวกับ CuttingMinigameController")]
    [SerializeField] private GameObject nextButtonObject;

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
        if (nextButtonObject != null)
            nextButtonObject.SetActive(false);
    }

    public void IncreaseProgress(float amount)
    {
        if (isCookingFinished) return;

        currentProgress += amount;
        currentProgress = Mathf.Clamp(currentProgress, 0f, maxProgress);

        Debug.Log($"[ระบบหลังบ้าน] กำลังผัดวัตถุดิบ... ความสุกภายใน: {currentProgress} / {maxProgress}");

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
        GameplayScore.Instance?.AddScore(100);
        Debug.Log("🍳✨ [SUCCESS] ผัดวัตถุดิบจนสุกได้ที่เรียบร้อย!");

        if (putDownButtonObject != null)
            putDownButtonObject.SetActive(true);

        if (nextButtonObject != null)
            nextButtonObject.SetActive(true);
    }

    public void ResetProgress()
    {
        currentProgress = 0f;
        isCookingFinished = false;

        if (nextButtonObject != null)
            nextButtonObject.SetActive(false);
    }
}
