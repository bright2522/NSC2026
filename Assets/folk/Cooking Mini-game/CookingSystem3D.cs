using UnityEngine;
using UnityEngine.UI;
using TMPro; // สำคัญมาก! ต้องใส่บรรทัดนี้เพื่อควบคุมตัวเลข TextMeshPro

public class CookingSystem3D : MonoBehaviour
{
    [Header("UI References (ลาก UI มาใส่ที่นี่)")]
    public RectTransform gaugeRect;         
    public Image arrowIndicator;            
    public Image cookingFillImage;          
    public TextMeshProUGUI timerText;          // ตัวเลขจับเวลาแบบข้อความดิจิทัล
    public TextMeshProUGUI cookingValueText;   // ตัวเลขเปอร์เซ็นต์ค่าความสุก
    [Tooltip("ลาก IdealZoneOverlay ที่คุณจัดตำแหน่งไว้ใน Editor มาใส่ที่นี่")]
    public RectTransform idealZoneOverlay;     // แถบเป้าหมาย UI

    [Header("3D Food Settings (ลากโมเดลอาหารมาใส่)")]
    public Renderer foodRenderer;           
    public Color normalColor = Color.white; 
    public Color burntColor = new Color(0.15f, 0.08f, 0.08f); 

    [Header("Cooking Speed Settings (ปรับความเร็ว)")]
    [Tooltip("ความเร็วการทำอาหารปกติเมื่ออยู่ในโซนอุดมคติ")]
    public float idealProgressSpeed = 3.0f;  
    [Tooltip("ความเร็วการทำอาหารที่ช้าลงอย่างมากเมื่ออยู่นอกโซน")]
    public float slowProgressSpeed = 0.15f;  

    [Header("Time Settings (ตั้งเวลาเกมเป็นวินาที)")]
    public float maxCookingTime = 35f;      
    private float currentTimer;

    [Header("Arrow Smooth Settings")]
    public float arrowSmoothSpeed = 60f; 

    [Header("Gameplay Settings (ดูค่าสถานะในเกม)")]
    public float currentHeat = 0f; 
    [HideInInspector] public float targetHeat = 0f; 

    [Header("Cooking Status (ดูค่าสถานะในเกม)")]
    [SerializeField] private float cookingProgress = 0f;
    private float maxCookingProgress = 100f;
    [Tooltip("เวลาชีวิตถ้าบิดไฟแรงเกินโซน (3 วินาทีก่อนไหม้)")]
    [SerializeField] private float burnTimer = 3f;   
    private float maxBurnTime = 3f;

    private bool isBurnt = false;
    private bool isCooked = false;
    private bool isTimeOut = false; 

    // ตัวแปรภายในสำหรับเก็บค่าโซนที่คำนวณได้จากตำแหน่ง UI จริง
    private float idealMin;
    private float idealMax;

    void Start()
    {
        if (foodRenderer != null) 
            foodRenderer.material.color = normalColor;

        currentTimer = maxCookingTime;

        // --- เรียกฟังก์ชันคำนวณโซนจากตำแหน่ง UI ที่คุณจัดไว้ ---
        CalculateZonesFromUI();
    }

    void Update()
    {
        if (isCooked || isBurnt || isTimeOut) return;

        // --- ระบบจับเวลาและควบคุมลูกศร (เดิม) ---
        currentTimer -= Time.deltaTime;
        if (currentTimer < 0) currentTimer = 0;
        UpdateTimerTextDisplay();
        if (currentTimer <= 0) { TriggerTimeOut(); return; }

        currentHeat = Mathf.MoveTowards(currentHeat, targetHeat, arrowSmoothSpeed * Time.deltaTime);
        UpdateHeatUI();

        // --- ตรรกะการทำอาหารและไหม้เกรียม (อ้างอิงตามโซนจาก UI จริง) ---
        if (currentHeat > idealMax)
        {
            // --- 1. โซนไฟแรงเกินแถบเป้าหมาย ---
            cookingProgress += slowProgressSpeed * Time.deltaTime; 
            burnTimer -= Time.deltaTime; 

            if (foodRenderer != null)
            {
                float burnRatio = 1f - (burnTimer / maxBurnTime);
                foodRenderer.material.color = Color.Lerp(normalColor, burntColor, burnRatio);
            }

            if (burnTimer <= 0) TriggerBurnt();
        }
        else if (currentHeat >= idealMin)
        {
            // --- 2. โซนในอุดมคติ (อยู่ภายในแถบพอดี) ---
            cookingProgress += idealProgressSpeed * Time.deltaTime; 
            CooldownBurnColor(); 
        }
        else
        {
            // --- 3. โซนไฟอ่อนเกินแถบเป้าหมาย ---
            cookingProgress += slowProgressSpeed * Time.deltaTime; 
            CooldownBurnColor(); 
        }

        // --- อัปเดต UI ความสุก ---
        if (cookingFillImage != null) cookingFillImage.fillAmount = cookingProgress / maxCookingProgress; 
        UpdateCookingValueDisplay();

        if (cookingProgress >= maxCookingProgress && !isBurnt) TriggerCooked();
    }

    // --- ฟังก์ชันใหม่: อ่านค่าตำแหน่งและขนาดของ UI จริงเพื่อแปลงเป็นค่าความร้อน 0-100 ---
    void CalculateZonesFromUI()
    {
        if (idealZoneOverlay != null && gaugeRect != null)
        {
            float gaugeHeight = gaugeRect.rect.height;

            // หาตำแหน่ง Y ของขอบบนและขอบล่างของแถบเป้าหมายจริงใน Unity Editor
            float overlayY = idealZoneOverlay.localPosition.y;
            float overlayHeight = idealZoneOverlay.rect.height;

            float yTop = overlayY + (overlayHeight / 2f);
            float yBottom = overlayY - (overlayHeight / 2f);

            // แปลงค่าพิกัดพิกเซล Y กลับมาเป็นค่าอุณหภูมิระบบเกม (0 - 100)
            idealMax = ((yTop + (gaugeHeight / 2f)) / gaugeHeight) * 100f;
            idealMin = ((yBottom + (gaugeHeight / 2f)) / gaugeHeight) * 100f;

            // ป้องกันไม่ให้ค่าหลุดขอบเกินโครงสร้างเกจ
            idealMax = Mathf.Clamp(idealMax, 0f, 100f);
            idealMin = Mathf.Clamp(idealMin, 0f, 100f);

            Debug.Log($"<color=cyan>ตั้งค่าโซนสำเร็จ! -> โซนปกติอยู่ที่อุณหภูมิเกม: {idealMin:F1} ถึง {idealMax:F1}</color>");
        }
        else
        {
            // ค่าสำรองกรณีลืมลาก UI มาใส่
            idealMin = 50f;
            idealMax = 70f;
        }
    }

    // --- ฟังก์ชันอัปเดต UI อื่นๆ ---
    void UpdateCookingValueDisplay()
    {
        if (cookingValueText != null)
        {
            int value = Mathf.RoundToInt(cookingProgress);
            value = Mathf.Clamp(value, 0, 100);
            cookingValueText.text = value.ToString();
        }
    }

    void UpdateTimerTextDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTimer / 60f);
            int seconds = Mathf.FloorToInt(currentTimer % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void CooldownBurnColor()
    {
        burnTimer = maxBurnTime; 
        if (foodRenderer != null)
        {
            foodRenderer.material.color = Color.Lerp(foodRenderer.material.color, normalColor, Time.deltaTime * 2f);
        }
    }

    void UpdateHeatUI()
    {
        if (arrowIndicator != null && gaugeRect != null)
        {
            float height = gaugeRect.rect.height;
            float targetY = ((currentHeat / 100f) * height) - (height / 2f);
            Vector3 newPos = arrowIndicator.rectTransform.localPosition;
            newPos.y = targetY;
            arrowIndicator.rectTransform.localPosition = newPos;
        }
    }

    void TriggerBurnt() { isBurnt = true; if (foodRenderer != null) foodRenderer.material.color = burntColor; Debug.Log("<color=red><b>อาหารไหม้เกรียม! Game Over</b></color>"); }
    void TriggerCooked() { isCooked = true; Debug.Log("<color=green><b>ทำอาหารเสร็จสมบูรณ์! Win!</b></color>"); }
    void TriggerTimeOut() { isTimeOut = true; Debug.Log("<color=yellow><b>หมดเวลาทำอาหาร! Game Over</b></color>"); }
}