using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

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

    [Header("UI Smooth Settings (LeanTween)")]
    public float cookingFillSmoothTime = 0.22f;
    public float arrowTweenSmoothTime = 0.14f;
    public float foodColorSmoothTime = 0.35f;
    public LeanTweenType uiEase = LeanTweenType.easeOutQuad;

    [Header("Gameplay Settings (ดูค่าสถานะในเกม)")]
    public float currentHeat = 0f; 
    [HideInInspector] public float targetHeat = 0f; 

    [Header("Cooking Status (ดูค่าสถานะในเกม)")]
    [SerializeField] private float cookingProgress = 0f;
    private float maxCookingProgress = 100f;
    [Tooltip("เวลาชีวิตถ้าบิดไฟแรงเกินโซน (3 วินาทีก่อนไหม้)")]
    [SerializeField] private float burnTimer = 3f;   
    private float maxBurnTime = 3f;

    public bool isBurnt = false;
    public bool isCooked = false;
    public bool isTimeOut = false;

    [Header("Events")]
    public UnityEvent whenStart = new UnityEvent();
    public UnityEvent whenEnd = new UnityEvent();

    private bool isPlaying;
    private bool hasInvokedEnd;

    // ตัวแปรภายในสำหรับเก็บค่าโซนที่คำนวณได้จากตำแหน่ง UI จริง
    private float idealMin;
    private float idealMax;

    private float displayedCookingFill;
    private int cookingFillTweenId = -1;
    private int heatArrowTweenId = -1;
    private int foodColorTweenId = -1;

    void Start()
    {
        CalculateZonesFromUI();
    }

    public void StartFunction()
    {
        hasInvokedEnd = false;
        isPlaying = true;
        isBurnt = false;
        isCooked = false;
        isTimeOut = false;
        cookingProgress = 0f;
        burnTimer = maxBurnTime;
        currentHeat = 0f;
        targetHeat = 0f;
        currentTimer = maxCookingTime;

        CancelTween(ref cookingFillTweenId);
        CancelTween(ref heatArrowTweenId);
        CancelTween(ref foodColorTweenId);

        if (foodRenderer != null)
            foodRenderer.material.color = normalColor;

        displayedCookingFill = 0f;
        if (cookingFillImage != null) cookingFillImage.fillAmount = 0f;
        UpdateTimerTextDisplay();
        UpdateCookingValueDisplay(0f);

        whenStart?.Invoke();
    }

    public void EndFunction()
    {
        if (hasInvokedEnd) return;

        hasInvokedEnd = true;
        isPlaying = false;

        CancelTween(ref cookingFillTweenId);
        CancelTween(ref heatArrowTweenId);
        CancelTween(ref foodColorTweenId);

        whenEnd?.Invoke();
    }

    void OnDestroy()
    {
        CancelTween(ref cookingFillTweenId);
        CancelTween(ref heatArrowTweenId);
        CancelTween(ref foodColorTweenId);
    }

    void Update()
    {
        if (!isPlaying || isCooked || isBurnt || isTimeOut) return;

        // --- ระบบจับเวลาและควบคุมลูกศร (เดิม) ---
        currentTimer -= Time.deltaTime;
        if (currentTimer < 0) currentTimer = 0;
        UpdateTimerTextDisplay();
        if (currentTimer <= 0) { TriggerTimeOut(); return; }

        currentHeat = Mathf.MoveTowards(currentHeat, targetHeat, arrowSmoothSpeed * Time.deltaTime);
        SmoothArrowPosition(currentHeat);

        Color foodTargetColor = normalColor;

        // --- ตรรกะการทำอาหารและไหม้เกรียม (อ้างอิงตามโซนจาก UI จริง) ---
        if (currentHeat > idealMax)
        {
            // --- 1. โซนไฟแรงเกินแถบเป้าหมาย ---
            cookingProgress += slowProgressSpeed * Time.deltaTime; 
            burnTimer -= Time.deltaTime; 

            float burnRatio = 1f - (burnTimer / maxBurnTime);
            foodTargetColor = Color.Lerp(normalColor, burntColor, burnRatio);

            if (burnTimer <= 0) TriggerBurnt();
        }
        else if (currentHeat >= idealMin)
        {
            // --- 2. โซนในอุดมคติ (อยู่ภายในแถบพอดี) ---
            cookingProgress += idealProgressSpeed * Time.deltaTime; 
            burnTimer = maxBurnTime;
        }
        else
        {
            // --- 3. โซนไฟอ่อนเกินแถบเป้าหมาย ---
            cookingProgress += slowProgressSpeed * Time.deltaTime; 
            burnTimer = maxBurnTime;
        }

        SmoothFoodColor(foodTargetColor);
        SmoothCookingFill(cookingProgress / maxCookingProgress);

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

    void CancelTween(ref int tweenId)
    {
        if (tweenId < 0) return;
        LeanTween.cancel(tweenId);
        tweenId = -1;
    }

    void SmoothCookingFill(float targetFill)
    {
        targetFill = Mathf.Clamp01(targetFill);
        if (Mathf.Approximately(displayedCookingFill, targetFill)) return;

        if (cookingFillTweenId >= 0 && LeanTween.isTweening(cookingFillTweenId))
            LeanTween.cancel(cookingFillTweenId);

        float startFill = displayedCookingFill;
        cookingFillTweenId = LeanTween.value(gameObject, startFill, targetFill, cookingFillSmoothTime)
            .setEase(uiEase)
            .setOnUpdate((float value) =>
            {
                displayedCookingFill = value;
                if (cookingFillImage != null) cookingFillImage.fillAmount = value;
                UpdateCookingValueDisplay(value);
            })
            .id;
    }

    void SmoothArrowPosition(float heat)
    {
        if (arrowIndicator == null || gaugeRect == null) return;

        float targetY = HeatToGaugeY(heat);
        float currentY = arrowIndicator.rectTransform.localPosition.y;
        if (Mathf.Approximately(currentY, targetY)) return;

        if (heatArrowTweenId >= 0 && LeanTween.isTweening(heatArrowTweenId))
            LeanTween.cancel(heatArrowTweenId);

        heatArrowTweenId = LeanTween.moveY(arrowIndicator.rectTransform, targetY, arrowTweenSmoothTime)
            .setEase(uiEase)
            .id;
    }

    void SmoothFoodColor(Color targetColor)
    {
        if (foodRenderer == null) return;

        Color currentColor = foodRenderer.material.color;
        if (ColorsAreClose(currentColor, targetColor)) return;

        if (foodColorTweenId >= 0 && LeanTween.isTweening(foodColorTweenId))
            LeanTween.cancel(foodColorTweenId);

        foodColorTweenId = LeanTween.value(gameObject, currentColor, targetColor, foodColorSmoothTime)
            .setEase(uiEase)
            .setOnUpdate((Color color) => foodRenderer.material.color = color)
            .id;
    }

    float HeatToGaugeY(float heat)
    {
        float height = gaugeRect.rect.height;
        return ((heat / 100f) * height) - (height / 2f);
    }

    static bool ColorsAreClose(Color a, Color b)
    {
        return Mathf.Approximately(a.r, b.r)
            && Mathf.Approximately(a.g, b.g)
            && Mathf.Approximately(a.b, b.b)
            && Mathf.Approximately(a.a, b.a);
    }

    void UpdateCookingValueDisplay(float progressValue)
    {
        if (cookingValueText == null) return;

        int value = Mathf.RoundToInt(progressValue * maxCookingProgress);
        value = Mathf.Clamp(value, 0, (int)maxCookingProgress);
        cookingValueText.text = value.ToString();
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

    void TriggerBurnt()
    {
        isBurnt = true;
        GameplayScore.Instance?.AddScore(10);
        CancelTween(ref foodColorTweenId);
        if (foodRenderer != null) foodRenderer.material.color = burntColor;
        Debug.Log("<color=red><b>อาหารไหม้เกรียม! Game Over</b></color>");
        EndFunction();
    }
    void TriggerCooked()
    {
        isCooked = true;
        GameplayScore.Instance?.AddScore(Mathf.RoundToInt(cookingProgress));
        Debug.Log("<color=green><b>ทำอาหารเสร็จสมบูรณ์! Win!</b></color>");
        EndFunction();
    }
    void TriggerTimeOut()
    {
        isTimeOut = true;
        Debug.Log("<color=yellow><b>หมดเวลาทำอาหาร! Game Over</b></color>");
        EndFunction();
    }
}