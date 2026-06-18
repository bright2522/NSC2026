using UnityEngine;
using UnityEngine.UI;
using TMPro; // สำคัญมาก! ต้องใส่บรรทัดนี้เพื่อควบคุมตัวเลข TextMeshPro

public class CookingSystem3D : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform gaugeRect;         
    public Image arrowIndicator;            
    public Image cookingFillImage;          
    
    [Header("New Timer Text (ลาก TimerText มาใส่ที่นี่)")]
    public TextMeshProUGUI timerText;       // ตัวเลขจับเวลาแบบข้อความดิจิทัล

    [Header("3D Food Settings")]
    public Renderer foodRenderer;           
    public Color normalColor = Color.white; 
    public Color burntColor = new Color(0.15f, 0.08f, 0.08f); 

    [Header("Cooking Speed Settings")]
    public float lowHeatProgressSpeed = 0.15f;  
    public float midHeatProgressSpeed = 2.4f;   
    public float highHeatProgressSpeed = 5.0f;  

    [Header("Time Settings (ตั้งเวลาเกมเป็นวินาที)")]
    public float maxCookingTime = 35f;      // ตั้งไว้ 35 วินาทีตามภาพเรฟของคุณ
    private float currentTimer;

    [Header("Arrow Smooth Settings")]
    public float arrowSmoothSpeed = 60f; 

    [Header("Gameplay Settings")]
    public float currentHeat = 0f; 
    [HideInInspector] public float targetHeat = 0f; 

    [Header("Cooking Status")]
    [SerializeField] private float cookingProgress = 0f;
    private float maxCookingProgress = 100f;
    [SerializeField] private float burnTimer = 3f;   
    private float maxBurnTime = 3f;

    private bool isBurnt = false;
    private bool isCooked = false;
    private bool isTimeOut = false; 

    void Start()
    {
        if (foodRenderer != null) 
            foodRenderer.material.color = normalColor;

        currentTimer = maxCookingTime;
    }

    void Update()
    {
        if (isCooked || isBurnt || isTimeOut) return;

        // --- ระบบนับเวลาถอยหลัง ---
        currentTimer -= Time.deltaTime;
        
        // บังคับไม่ให้เวลาต่ำกว่า 0
        if (currentTimer < 0) currentTimer = 0;

        // อัปเดตตัวเลขดิจิทัลบนหน้าจอ (แปลงจากวินาที เป็น นาที:วินาที)
        UpdateTimerTextDisplay();

        // ถ้าเวลาหมด = แพ้
        if (currentTimer <= 0)
        {
            TriggerTimeOut();
            return;
        }

        // --- ระบบควบคุมลูกศรหน่วง ---
        currentHeat = Mathf.MoveTowards(currentHeat, targetHeat, arrowSmoothSpeed * Time.deltaTime);
        UpdateHeatUI();

        // --- ตรรกะความเร็วการทำอาหาร ---
        if (currentHeat <= 40f) 
        {
            cookingProgress += lowHeatProgressSpeed * Time.deltaTime; 
            CooldownBurnColor(); 
        }
        else if (currentHeat > 40f && currentHeat <= 80f) 
        {
            cookingProgress += midHeatProgressSpeed * Time.deltaTime; 
            CooldownBurnColor();
        }
        else if (currentHeat > 80f) 
        {
            cookingProgress += highHeatProgressSpeed * Time.deltaTime; 
            burnTimer -= Time.deltaTime; 

            if (foodRenderer != null)
            {
                float burnRatio = 1f - (burnTimer / maxBurnTime);
                foodRenderer.material.color = Color.Lerp(normalColor, burntColor, burnRatio);
            }

            if (burnTimer <= 0) TriggerBurnt();
        }

        // อัปเดตหลอดความสุก
        if (cookingFillImage != null) 
        {
            cookingFillImage.fillAmount = cookingProgress / maxCookingProgress; 
        }

        if (cookingProgress >= maxCookingProgress && !isBurnt) 
        {
            TriggerCooked();
        }
    }

    // ฟังก์ชันคำนวณและแปลงค่าเวลาให้ออกมาเป็นรูปแบบ 00:35
    void UpdateTimerTextDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTimer / 60f);
            int seconds = Mathf.FloorToInt(currentTimer % 60f);
            
            // ใช้ string.Format ล็อกให้แสดงผลเป็นตัวเลข 2 หลักเสมอ
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

    void TriggerBurnt()
    {
        isBurnt = true;
        if (foodRenderer != null) foodRenderer.material.color = burntColor; 
        Debug.Log("<color=red><b>อาหารไหม้เกรียม! Game Over</b></color>");
    }

    void TriggerCooked()
    {
        isCooked = true;
        Debug.Log("<color=green><b>ทำอาหารเสร็จสมบูรณ์! Win!</b></color>");
    }

    void TriggerTimeOut()
    {
        isTimeOut = true;
        Debug.Log("<color=yellow><b>หมดเวลาทำอาหาร! Game Over</b></color>");
    }
}