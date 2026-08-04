using UnityEngine;

public class SwipeStationSlider : MonoBehaviour
{
    public static SwipeStationSlider Instance { get; private set; }

    [Header("ระยะห่างระหว่างสเตชัน (world units)")]
    public float spacing = 20f;

    [Header("จำนวนสเตชัน")]
    public int stationCount = 3;
    public int startIndex = 0;

    [Header("ความลื่น")]
    public float snapSpeed = 10f;
    [Tooltip("ลากเกินเศษนี้ของ 'ความกว้างจอ' ถึงจะเปลี่ยนหน้า (0.2 = ลาก 20% ของความกว้างจอ)")]
    public float switchThreshold = 0.2f;
    [Tooltip("ปัดเร็ว ๆ (flick) เปลี่ยนหน้าได้แม้ลากไม่ไกล — หน่วยเป็นพิกเซล/วินาที")]
    public float flickSpeedPixels = 600f;

    [Header("เปิด/ปิดการเลื่อน")]
    public bool swipeEnabled = false; // 👈 ตั้ง Default ไว้เป็น false (ล็อกไว้ก่อน)

    [Header("ระบบจำกัดการเลื่อนแค่ 1 ครั้ง")]
    public bool allowOneSwipeOnly = false; 

    private int current;
    private Vector3 homePosition;
    private Vector3 targetPosition;
    private Camera cam;

    private bool dragging;
    private float dragStartWorldX;
    private float dragStartScreenX;
    private Vector3 dragStartRowPos;
    private float lastScreenX;
    private float velocity;

    // --- เพิ่มตัวแปรสำหรับนับการเลื่อนหน้าจอ ---
    private int successfulSwipeCount = 0; 

    // --- ป้องกันการสร้าง Loop ซ้ำในเฟรมเดียวกัน ---
    private bool hasTriggeredLoop = false;

    private void Awake()
    {
        // 💡 ปรับปรุง: ย้าย Instance มาชี้ที่สเตชันใหม่ล่าสุดเสมอ (ไม่สั่ง Destroy)
        Instance = this;
    }

    private void OnEnable()
    {
        // 🔄 รีเซ็ตค่าป้องกันการสร้าง Loop ซ้ำทุกครั้งที่ Component ถูกเปิดใช้งาน
        hasTriggeredLoop = false;
    }

    void Start()
    {
        cam = Camera.main;
        homePosition = transform.position;
        current = Mathf.Clamp(startIndex, 0, stationCount - 1);
        targetPosition = PositionForIndex(current);
        transform.position = targetPosition;
    }

    void Update()
    {
        // 🔒 ห้ามเลื่อนสไลด์ระหว่างนับเวลาถอยหลังก่อนเริ่มเกม
        bool locked = CountdownController.IsCountdownActive ||
            (MultiplayerGameManager.Instance != null &&
             (!MultiplayerGameManager.IsSpawnedReady || !MultiplayerGameManager.Instance.GameStarted));
        if (locked)
        {
            if (!dragging)
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * snapSpeed);
            return;
        }

        // ถ้า swipeEnabled เป็น false จะไม่รับ Input การลากเลย
        if (swipeEnabled)
        {
            HandleDrag();
        }

        // ค่อยๆ Lerp หน้าจอไปยังเป้าหมายเสมอ
        if (!dragging)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * snapSpeed);
        }
    }

    void HandleDrag()
    {
        if (GetPressDown(out Vector2 downPos))
        {
            if (PanDragCoordinator.HasActiveInteraction) return;

            dragging = true;
            dragStartWorldX = ScreenToWorldX(downPos);
            dragStartScreenX = downPos.x;
            lastScreenX = dragStartScreenX;
            dragStartRowPos = transform.position;
            velocity = 0f;
        }

        if (dragging && GetPressHeld(out Vector2 movePos))
        {
            if (KnifeMovement.IsAnyKnifeDragging || PanDragCoordinator.HasActiveInteraction)
            {
                CancelDrag();
                return;
            }

            float wx = ScreenToWorldX(movePos);
            float delta = wx - dragStartWorldX;
            transform.position = new Vector3(dragStartRowPos.x + delta, dragStartRowPos.y, dragStartRowPos.z);

            velocity = (movePos.x - lastScreenX) / Mathf.Max(Time.deltaTime, 0.0001f);
            lastScreenX = movePos.x;
        }

        if (dragging && GetPressUp())
        {
            CompleteSwipe();
        }
    }

    private void CompleteSwipe()
    {
        dragging = false;

        float screenMoved = Mathf.Abs(lastScreenX - dragStartScreenX);
        float screenFraction = screenMoved / Mathf.Max(Screen.width, 1f);

        // คำนวณระยะทางและทิศทางที่ลาก (ค่าเป็นบวกเมื่อลากไปทางซ้ายเพื่อไปหน้าถัดไป)
        float draggedDelta = dragStartWorldX - transform.position.x; 
        bool flick = Mathf.Abs(velocity) > flickSpeedPixels;

        int oldIndex = current;

        // เปลี่ยนหน้าถ้าปัดเกินระยะกำหนด
        if (screenFraction > switchThreshold || flick)
        {
            if (draggedDelta > 0.05f) current = NextIndex();
            else if (draggedDelta < -0.05f) current = PreviousIndex();
        }

        targetPosition = PositionForIndex(current);

        // 🎯 1. เมื่อเลื่อนเปลี่ยนหน้าสำเร็จจริง (Index เปลี่ยน)
        if (current != oldIndex)
        {
            // --- นับการเลื่อนหน้าจอและเพิ่มคะแนน ---
            successfulSwipeCount++;

            if (ScoreManager.Instance != null)
            {
                if (successfulSwipeCount == 1)
                {
                    ScoreManager.Instance.AddScore(100);
                    Debug.Log("<color=green>เลื่อนหน้าจอครั้งแรก ได้คะแนน +100</color>");
                }
                else if (successfulSwipeCount == 2)
                {
                    ScoreManager.Instance.AddScore(40);
                    Debug.Log("<color=green>เลื่อนหน้าจอครั้งที่สอง ได้คะแนน +40</color>");
                }
            }

            // 🖼️ สั่งซ่อนรูปภาพ / ปุ่ม Next
            if (SliceProgress.Instance != null)
            {
                SliceProgress.Instance.HideNextButton();
            }

            // 🔒 สั่งล็อกหน้าจอทันทีหากอยู่ในโหมดปัด 1 ครั้ง
            if (allowOneSwipeOnly)
            {
                allowOneSwipeOnly = false;
                swipeEnabled = false;
                Debug.Log("<color=orange>🔒 ปัดเปลี่ยนหน้าสำเร็จแล้ว สั่งล็อกการเลื่อนจอทันที!</color>");
            }
        }

        // 🔄 2. ตรวจสอบการสร้าง Loop: เมื่ออยู่สเตชันสุดท้าย แล้วพยายามปัดไปสเตชันถัดไปอีก
        bool isAtLastStation = (oldIndex == stationCount - 1);
        bool swipingForward = (draggedDelta > 0.05f || (flick && velocity < 0));

        if (isAtLastStation && swipingForward && !hasTriggeredLoop)
        {
            hasTriggeredLoop = true;

            StationLoopManager loopManager = StationLoopManager.Instance;
            if (loopManager == null)
            {
                loopManager = FindFirstObjectByType<StationLoopManager>();
            }

            if (loopManager != null)
            {
                Debug.Log("<color=cyan><b>🚀 ปัดที่สเตชันสุดท้ายแล้ว! กำลังเรียก StationLoopManager ให้สร้างชุดใหม่...</b></color>");
                loopManager.SpawnNextStation();
            }
            else
            {
                Debug.LogError("<color=red>❌ ไม่พบ StationLoopManager ใน Scene! กรุณาวาง GameObject ที่แปะ StationLoopManager ไว้ใน Scene หลัก</color>");
            }
        }
    }

    private void CancelDrag()
    {
        dragging = false;
        transform.position = dragStartRowPos;
        targetPosition = PositionForIndex(current);
    }

    Vector3 PositionForIndex(int index) => homePosition - StationAxis() * (spacing * index);

    Vector3 StationAxis()
    {
        Vector3 axis = transform.rotation * Vector3.right;
        return Mathf.Abs(axis.x) < 0.001f ? Vector3.right : Vector3.right * Mathf.Sign(axis.x);
    }

    float ScreenToWorldX(Vector2 screenPos)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return 0f;

        float depth = Vector3.Dot(transform.position - cam.transform.position, cam.transform.forward);
        return cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, depth)).x;
    }

    public void Next() { current = NextIndex(); targetPosition = PositionForIndex(current); }
    public void Previous() { current = PreviousIndex(); targetPosition = PositionForIndex(current); }
    
    public void SetSwipeEnabled(bool v) 
    { 
        swipeEnabled = v; 
    }

    /// <summary>
    /// เรียกฟังก์ชันนี้หลังหั่นไส้กรอกเสร็จ
    /// </summary>
    public void EnableOneTimeSwipe()
    {
        allowOneSwipeOnly = true;
        swipeEnabled = true;
        Debug.Log("<color=yellow>🔓 ปลดล็อกให้ปัดเปลี่ยนหน้าได้ 1 ครั้ง</color>");
    }

    public void GoToIndex(int index)
    {
        current = Mathf.Clamp(index, 0, stationCount - 1);
        targetPosition = PositionForIndex(current);
    }

    int NextIndex()
    {
        if (stationCount <= 0) return 0;
        return Mathf.Min(current + 1, stationCount - 1);
    }

    int PreviousIndex()
    {
        if (stationCount <= 0) return 0;
        return Mathf.Max(current - 1, 0);
    }

    bool GetPressDown(out Vector2 pos)
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) { pos = Input.GetTouch(0).position; return true; }
        if (Input.touchCount == 0 && Input.GetMouseButtonDown(0)) { pos = Input.mousePosition; return true; }
        pos = Vector2.zero; return false;
    }

    bool GetPressHeld(out Vector2 pos)
    {
        if (Input.touchCount > 0) { pos = Input.GetTouch(0).position; return true; }
        if (Input.GetMouseButton(0)) { pos = Input.mousePosition; return true; }
        pos = Vector2.zero; return false;
    }

    bool GetPressUp()
    {
        if (Input.touchCount > 0)
        {
            var ph = Input.GetTouch(0).phase;
            return ph == TouchPhase.Ended || ph == TouchPhase.Canceled;
        }
        return Input.GetMouseButtonUp(0);
    }
}