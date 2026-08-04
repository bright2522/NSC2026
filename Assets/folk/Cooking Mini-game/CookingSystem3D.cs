using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class CookingSystem3D : MonoBehaviour
{
    [Header("🎯 UI Parent Panel (วัตถุแม่ที่รวม UI ทั้งหมด)")]
    [Tooltip("ลาก Panel หรือ GameObject แม่ที่รวม UI ทั้งหมดของระบบทำอาหารมาใส่ที่นี่")]
    public GameObject uiPanel; 

    [Header("UI References (ลาก UI มาใส่ที่นี่)")]
    public RectTransform gaugeRect;         
    public Image arrowIndicator;            
    public Image cookingFillImage;          
    public TextMeshProUGUI timerText;          
    public TextMeshProUGUI cookingValueText;   
    [Tooltip("ลาก IdealZoneOverlay ที่คุณจัดตำแหน่งไว้ใน Editor มาใส่ที่นี่")]
    public RectTransform idealZoneOverlay;    

    [Header("3D Food Settings (ลากโมเดลอาหารมาใส่)")]
    public Renderer foodRenderer;          
    public Color normalColor = Color.white; 
    public Color burntColor = new Color(0.15f, 0.08f, 0.08f); 

    [Header("✨ Spawn New Object Settings (เสกออบเจกต์ใหม่เมื่อสุก)")]
    [Tooltip("ลาก Prefab อาหารสุก หรือออบเจกต์ที่เตรียมไว้ที่จะให้เสกออกมาใส่ที่นี่")]
    public GameObject cookedPrefabToSpawn;

    [Tooltip("จุดที่จะให้เสกออบเจกต์ออกมา (ถ้าไม่ใส่จะเสกตรงจุด clearAreaCenter)")]
    public Transform spawnPoint;

    [Tooltip("🎯 ลาก Transform แม่ (Parent) ที่ต้องการให้วัตถุใหม่เข้าไปเป็นลูกมาใส่ที่นี่")]
    public Transform spawnParent;

    [Header("🧹 Area & Target Clear Settings (ตั้งค่าการลบโมเดลในกระทะ)")]
    [Tooltip("Tag ของอาหาร/วัตถุดิบ เช่น 'Food' (หากตั้งไว้ ระบบจะลบวัตถุที่มี Tag นี้ทั้งหมดทันทีเมื่อสุก)")]
    public string foodTag = "Food";

    [Tooltip("จุดศูนย์กลางของพื้นที่ที่จะสแกนลบ (ลากกระทะ หรือ Transform ศูนย์กลางกระทะมาใส่)")]
    public Transform clearAreaCenter;
    
    [Tooltip("ขนาดของแอเรียกล่องที่จะคลุมกระทะ (กว้าง x สูง x ลึก)")]
    public Vector3 clearAreaSize = new Vector3(2f, 2f, 2f);

    [Tooltip("ลาก GameObject กระทะหลักมาใส่ที่นี่")]
    public GameObject panObjectToIgnore;

    [Tooltip("🛡️ เพิ่มวัตถุอื่นๆ ที่ไม่อยากให้ถูกลบ/สแกนโดนที่นี่ (เช่น เตา, ตะกร้า, เครื่องปรุง)")]
    public List<GameObject> additionalObjectsToIgnore = new List<GameObject>();

    [Tooltip("🎯 ติ๊กถูกหากต้องการแค่ 'ซ่อน' กระทะ (Disable) เมื่อหลอดเต็ม หรือเอาติ๊กออกเพื่อ 'ลบกระทะทิ้ง' (Destroy)")]
    public bool hidePanOnCooked = true;

    [Tooltip("Layer ของวัตถุที่จะถูกลบ (ถ้าตั้งไว้เป็น Everything จะตรวจจับทั้งหมด)")]
    public LayerMask objectsToClearLayer = ~0; 

    [Header("Cooking Speed Settings (ปรับความเร็ว)")]
    public float idealProgressSpeed = 3.0f;  
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

    [Header("Gameplay Settings")]
    public float currentHeat = 0f; 
    [HideInInspector] public float targetHeat = 0f; 

    [Header("Cooking Status")]
    [SerializeField] private float cookingProgress = 0f;
    private float maxCookingProgress = 100f;
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
    private bool waitingForIngredients;

    private float idealMin;
    private float idealMax;

    private float displayedCookingFill;
    private int cookingFillTweenId = -1;
    private int heatArrowTweenId = -1;
    private int foodColorTweenId = -1;

    void Awake()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
    }

    void Start()
    {
        CalculateZonesFromUI();
    }

    public void StartFunction()
    {
        // 🍳 แสดงกระทะกลับมาเมื่อเริ่มรอบใหม่
        if (panObjectToIgnore != null)
        {
            panObjectToIgnore.SetActive(true);
        }

        hasInvokedEnd = false;

        // 🔒 รอให้ใส่วัตถุดิบครบก่อน ค่อยเปิด UI ควบคุมไฟ (ดูสถานะจาก PanPrepManager)
        bool prepDone = PanPrepManager.Instance != null && PanPrepManager.Instance.IsAllPrepDone;
        waitingForIngredients = !prepDone;
        isPlaying = prepDone;

        if (uiPanel != null)
        {
            uiPanel.SetActive(prepDone);
        }

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
        waitingForIngredients = false;

        CancelTween(ref cookingFillTweenId);
        CancelTween(ref heatArrowTweenId);
        CancelTween(ref foodColorTweenId);

        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }

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
        if (waitingForIngredients)
        {
            if (PanPrepManager.Instance != null && PanPrepManager.Instance.IsAllPrepDone)
            {
                waitingForIngredients = false;
                isPlaying = true;
                currentTimer = maxCookingTime;
                if (uiPanel != null) uiPanel.SetActive(true);
            }
            return;
        }

        if (!isPlaying || isCooked || isBurnt || isTimeOut) return;

        currentTimer -= Time.deltaTime;
        if (currentTimer < 0) currentTimer = 0;
        UpdateTimerTextDisplay();
        if (currentTimer <= 0) { TriggerTimeOut(); return; }

        currentHeat = Mathf.MoveTowards(currentHeat, targetHeat, arrowSmoothSpeed * Time.deltaTime);
        SmoothArrowPosition(currentHeat);

        Color foodTargetColor = normalColor;

        // 🥄 % การทำอาหาร (cookingProgress) จะขยับก็ต่อเมื่อกำลังผัดด้วยตะหลิวจริงๆ เท่านั้น
        bool isStirring = SpatulaController.IsStirringNow;

        if (currentHeat > idealMax)
        {
            if (isStirring) cookingProgress += slowProgressSpeed * Time.deltaTime;
            burnTimer -= Time.deltaTime;

            float burnRatio = 1f - (burnTimer / maxBurnTime);
            foodTargetColor = Color.Lerp(normalColor, burntColor, burnRatio);

            if (burnTimer <= 0) TriggerBurnt();
        }
        else if (currentHeat >= idealMin)
        {
            if (isStirring) cookingProgress += idealProgressSpeed * Time.deltaTime;
            burnTimer = maxBurnTime;
        }
        else
        {
            if (isStirring) cookingProgress += slowProgressSpeed * Time.deltaTime;
            burnTimer = maxBurnTime;
        }

        SmoothFoodColor(foodTargetColor);
        SmoothCookingFill(cookingProgress / maxCookingProgress);

        // 🎯 เช็คเมื่อหลอดเต็ม (cookingProgress >= 100)
        if (cookingProgress >= maxCookingProgress && !isBurnt)
        {
            TriggerCooked();
        }
    }

    void CalculateZonesFromUI()
    {
        if (idealZoneOverlay != null && gaugeRect != null)
        {
            float gaugeHeight = gaugeRect.rect.height;

            float overlayY = idealZoneOverlay.localPosition.y;
            float overlayHeight = idealZoneOverlay.rect.height;

            float yTop = overlayY + (overlayHeight / 2f);
            float yBottom = overlayY - (overlayHeight / 2f);

            idealMax = ((yTop + (gaugeHeight / 2f)) / gaugeHeight) * 100f;
            idealMin = ((yBottom + (gaugeHeight / 2f)) / gaugeHeight) * 100f;

            idealMax = Mathf.Clamp(idealMax, 0f, 100f);
            idealMin = Mathf.Clamp(idealMin, 0f, 100f);
        }
        else
        {
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

    // 🎯 ฟังก์ชันสแกนและลบวัตถุเก่าออกจากกระทะ (พร้อมระบบกรอง Ignore)
    void ClearPanObjects()
    {
        if (!string.IsNullOrEmpty(foodTag))
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(foodTag);
            foreach (GameObject obj in taggedObjects)
            {
                if (!IsObjectIgnored(obj))
                {
                    Destroy(obj);
                }
            }
        }

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("FriedEgg_Combined") || obj.name.Contains("FriedEgg"))
            {
                if (!IsObjectIgnored(obj))
                {
                    Destroy(obj);
                }
            }
        }

        Transform centerPoint = (clearAreaCenter != null) ? clearAreaCenter : transform;
        Collider[] hitColliders = Physics.OverlapBox(
            centerPoint.position, 
            clearAreaSize / 2f, 
            centerPoint.rotation, 
            objectsToClearLayer
        );

        foreach (Collider col in hitColliders)
        {
            GameObject objToDestroy = col.gameObject;

            if (IsObjectIgnored(objToDestroy))
            {
                continue; 
            }

            Transform rootParent = objToDestroy.transform;
            while (rootParent.parent != null && rootParent.parent != centerPoint)
            {
                if (IsObjectIgnored(rootParent.parent.gameObject)) break;
                rootParent = rootParent.parent;
            }

            if (!IsObjectIgnored(rootParent.gameObject))
            {
                Destroy(rootParent.gameObject);
            }
        }

        Debug.Log("<color=cyan>🧹 ลบวัตถุเก่าเรียบร้อยแล้ว (ข้ามวัตถุที่อยู่ในรายการ Ignore)</color>");
    }

    private bool IsObjectIgnored(GameObject obj)
    {
        if (obj == null) return true;

        if (panObjectToIgnore != null)
        {
            if (obj == panObjectToIgnore || obj.transform.IsChildOf(panObjectToIgnore.transform))
            {
                return true;
            }
        }

        foreach (GameObject ignoredObj in additionalObjectsToIgnore)
        {
            if (ignoredObj != null)
            {
                if (obj == ignoredObj || obj.transform.IsChildOf(ignoredObj.transform))
                {
                    return true;
                }
            }
        }

        return false;
    }

    void HandlePanRemoval()
    {
        if (panObjectToIgnore != null)
        {
            if (hidePanOnCooked)
            {
                panObjectToIgnore.SetActive(false); 
                Debug.Log("<color=yellow>🍳 หลอดเต็มแล้ว: ซ่อนกระทะเรียบร้อย</color>");
            }
            else
            {
                Destroy(panObjectToIgnore); 
                Debug.Log("<color=red>🍳 หลอดเต็มแล้ว: ลบกระทะออกเรียบร้อย</color>");
            }
        }
    }

    void SpawnCookedObject()
    {
        if (cookedPrefabToSpawn != null)
        {
            Vector3 targetPosition = transform.position;
            Quaternion targetRotation = Quaternion.identity;

            if (spawnPoint != null)
            {
                targetPosition = spawnPoint.position;
                targetRotation = spawnPoint.rotation;
            }
            else if (clearAreaCenter != null)
            {
                targetPosition = clearAreaCenter.position;
                targetRotation = clearAreaCenter.rotation;
            }

            GameObject spawnedObj;

            if (spawnParent != null)
            {
                spawnedObj = Instantiate(cookedPrefabToSpawn, targetPosition, targetRotation, spawnParent);
            }
            else
            {
                spawnedObj = Instantiate(cookedPrefabToSpawn, targetPosition, targetRotation);
            }

            Debug.Log($"<color=yellow>✨ เสกวัตถุใหม่เรียบร้อยแล้ว: {spawnedObj.name}</color>");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform centerPoint = (clearAreaCenter != null) ? clearAreaCenter : transform;
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        Gizmos.matrix = Matrix4x4.TRS(centerPoint.position, centerPoint.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, clearAreaSize);
        Gizmos.DrawWireCube(Vector3.zero, clearAreaSize);
    }

    void TriggerBurnt()
    {
        isBurnt = true;
        // ถ้าต้องการให้ระบบไหม้บวกคะแนนด้วย (สามารถเปลี่ยนหรือลบได้ตามต้องการ)
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(10);
        }
        
        CancelTween(ref foodColorTweenId);
        if (foodRenderer != null) foodRenderer.material.color = burntColor;
        Debug.Log("<color=red><b>อาหารไหม้เกรียม! Game Over</b></color>");
        EndFunction();
    }

    void TriggerCooked()
    {
        isCooked = true;

        // 🎯 เชื่อมกับ ScoreManager บวกคะแนน +100 เมื่อหลอดทำอาหารเต็ม 100
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(100);
            Debug.Log("<color=green><b>หลอดทำอาหารเต็ม! บวกคะแนน +100 สำเร็จ</b></color>");
        }
        else
        {
            Debug.LogWarning("หา ScoreManager ไม่เจอในฉาก กรุณาตรวจสอบ!");
        }

        Debug.Log("<color=green><b>ทำอาหารเสร็จสมบูรณ์! Win!</b></color>");

        // 1. ลบของเก่าในกระทะ
        ClearPanObjects();

        // 2. ซ่อน/ลบกระทะออก (กระทะจะหายไปทันทีเมื่อหลอดเต็ม)
        HandlePanRemoval();

        // 3. เสกของใหม่ใส่เข้าไปใน Spawn Parent
        SpawnCookedObject();

        // 4. ปลดล็อกการปัดหน้าจอ
        if (SwipeStationSlider.Instance != null)
        {
            SwipeStationSlider.Instance.SetSwipeEnabled(true);
        }

        EndFunction();
    }

    void TriggerTimeOut()
    {
        isTimeOut = true;
        Debug.Log("<color=yellow><b>หมดเวลาทำอาหาร! Game Over</b></color>");
        EndFunction();
    }
}