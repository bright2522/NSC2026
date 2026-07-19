using UnityEngine;

// เลื่อนแบบหน้าจอมือถือ: ทั้งแถว (พร้อมของทุกอย่าง) เลื่อนตามนิ้ว ปล่อยแล้ว snap เข้าหน้า
// ** ตัวนี้ต้องเป็นแม่ของทุกสเตชัน และสเตชันวางเรียงตามแกน X ห่างเท่า spacing **
public class SwipeStationSlider : MonoBehaviour
{
    [Header("ระยะห่างระหว่างสเตชัน (world units)")]
    public float spacing = 20f;

    [Header("จำนวนสเตชัน")]
    public int stationCount = 3;
    public int startIndex = 0;

    [Header("ความลื่น")]
    public float snapSpeed = 10f;
    [Tooltip("ลากเกินเศษนี้ของ 'ความกว้างจอ' ถึงจะเปลี่ยนหน้า (0.2 = ลาก 20% ของความกว้างจอ)")]
    public float switchThreshold = 0.2f;
    // เปลี่ยนชื่อ field จาก flickSpeed เดิม (หน่วย world-unit/วิ) เพื่อทิ้งค่าเก่าที่ serialize ไว้ในซีน
    // ไม่ให้ค่าเก่า (เช่น 4) มาทับ default ใหม่ที่เป็นพิกเซล/วินาที
    [Tooltip("ปัดเร็ว ๆ (flick) เปลี่ยนหน้าได้แม้ลากไม่ไกล — หน่วยเป็นพิกเซล/วินาที")]
    public float flickSpeedPixels = 600f;

    [Header("เปิด/ปิดการเลื่อน (ปิดตอนหั่น)")]
    public bool swipeEnabled = true;

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
        if (swipeEnabled) HandleDrag();

        if (!dragging)
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * snapSpeed);
    }

    void HandleDrag()
    {
        if (GetPressDown(out Vector2 downPos))
        {
            // ถ้ามีของในซีน pad ถูกจับอยู่ (ถ้วยไส้กรอก/ตะหลิว/ไข่/กระเทียม ผ่าน PanDragCoordinator)
            // ห้ามเริ่ม swipe — ไม่งั้นแถวจะเลื่อนหนีระหว่างผู้เล่นลากของใส่กระทะ
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
            // ของอื่นมีสิทธิ์ก่อน swipe: ทั้งมีด (KnifeMovement) และ draggable ในซีน pad
            // (PanDragCoordinator อาจถูกจับทีหลังในเฟรมเดียวกัน เพราะลำดับ Update ไม่แน่นอน
            //  จึงต้องเช็คระหว่างลากด้วย แล้วยกเลิก swipe คืนตำแหน่งเดิมทันที)
            if (KnifeMovement.IsAnyKnifeDragging || PanDragCoordinator.HasActiveInteraction)
            {
                dragging = false;
                transform.position = dragStartRowPos;
                targetPosition = PositionForIndex(current);
                return;
            }

            float wx = ScreenToWorldX(movePos);
            float delta = wx - dragStartWorldX;
            transform.position = new Vector3(dragStartRowPos.x + delta, dragStartRowPos.y, dragStartRowPos.z);

            // ใช้ความเร็วแบบพิกเซลจอ ไม่ใช่ world-unit เพราะ world-unit ขึ้นกับกล้อง/ระยะ
            // ทำให้ threshold เพี้ยนไปตามฉาก (ลากยากหรือง่ายเกินไปแบบสุ่ม)
            velocity = (movePos.x - lastScreenX) / Mathf.Max(Time.deltaTime, 0.0001f);
            lastScreenX = movePos.x;
        }

        if (dragging && GetPressUp())
        {
            dragging = false;

            // ระยะที่ลากตัดสินด้วยพิกเซลจอ (สม่ำเสมอทุกกล้อง/ทุกจอ)
            float screenMoved = Mathf.Abs(lastScreenX - dragStartScreenX);
            float screenFraction = screenMoved / Mathf.Max(Screen.width, 1f);

            // ทิศทางตัดสินจากการเคลื่อนของแถวจริง ๆ เทียบกับแนวเรียงสเตชัน
            // (ไม่เดาจากทิศจอ เพราะกล้องหมุน 180° ทำให้ซ้าย-ขวากลับด้านได้)
            // ค่าบวก = ลากให้สเตชัน index สูงขึ้นเข้ามาในจอ
            float draggedTowardNext = Vector3.Dot(dragStartRowPos - transform.position, StationAxis()) / spacing;

            // ลากไกลพอ หรือ ปัดเร็ว (flick) = เปลี่ยนหน้า
            bool flick = Mathf.Abs(velocity) > flickSpeedPixels;

            if (screenFraction > switchThreshold || flick)
            {
                if (draggedTowardNext > 0.001f) current = NextIndex();
                else if (draggedTowardNext < -0.001f) current = PreviousIndex();
            }

            targetPosition = PositionForIndex(current);
        }
    }

    // แถวต้องเลื่อนสวนทางกับแนวเรียงสเตชัน เพื่อดึงสเตชันถัดไปเข้ามากลางจอ
    Vector3 PositionForIndex(int index) => homePosition - StationAxis() * (spacing * index);

    // แกนที่สเตชันเรียงต่อกัน: ลูก ๆ ของแถววางห่างกันตาม local +X และแถวเลื่อนเฉพาะแกน X โลก
    Vector3 StationAxis()
    {
        Vector3 axis = transform.rotation * Vector3.right;
        return Mathf.Abs(axis.x) < 0.001f ? Vector3.right : Vector3.right * Mathf.Sign(axis.x);
    }

    float ScreenToWorldX(Vector2 screenPos)
    {
        if (cam == null) cam = Camera.main;

        // ระยะที่ ScreenToWorldPoint ต้องการ คือระยะห่างตามแกน forward ของกล้อง ไม่ใช่ผลต่างแกน Z ตรง ๆ
        // กล้องในฉากนี้ก้มมองลงเกือบ 90 องศา (forward ชี้ลงตามแกน Y เป็นหลัก) ถ้าใช้ผลต่าง Z
        // จะได้ค่า depth ที่เพี้ยนไปมาก ทำให้การลากตามนิ้วไม่แม่นยำ/ไม่ขยับตามที่ลาก
        float depth = Vector3.Dot(transform.position - cam.transform.position, cam.transform.forward);
        return cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, depth)).x;
    }

    public void Next()     { current = NextIndex();     targetPosition = PositionForIndex(current); }
    public void Previous() { current = PreviousIndex(); targetPosition = PositionForIndex(current); }
    public void SetSwipeEnabled(bool v) => swipeEnabled = v;

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
