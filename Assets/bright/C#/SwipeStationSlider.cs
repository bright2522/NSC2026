using UnityEngine;

// เลื่อนแบบหน้าจอมือถือ: ทั้งแถว (พร้อมของทุกอย่าง) เลื่อนตามนิ้ว ปล่อยแล้ว snap เข้าหน้า
// ** ตัวนี้ต้องเป็นแม่ของทุกสเตชัน และสเตชันวางเรียงตามแกน X ห่างเท่า spacing **
public class SwipeStationSlider : MonoBehaviour
{
    [Header("ระยะห่างระหว่างสเตชัน (world units)")]
    public float spacing = 20f;

    [Header("จำนวนสเตชัน")]
    public int stationCount = 3;
    public int startIndex = 1;

    [Header("ความลื่น")]
    public float snapSpeed = 10f;
    [Tooltip("ลากเกินเศษนี้ของ 1 หน้า ถึงจะเปลี่ยนหน้า (0.2 = ปัดนิดเดียวก็เปลี่ยน)")]
    public float switchThreshold = 0.2f;
    [Tooltip("ปัดเร็ว ๆ (flick) เปลี่ยนหน้าได้แม้ลากไม่ไกล")]
    public float flickSpeed = 4f;

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
    private float lastWorldX;
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
            dragging = true;
            dragStartWorldX = ScreenToWorldX(downPos);
            dragStartScreenX = downPos.x;
            lastWorldX = dragStartWorldX;
            lastScreenX = dragStartScreenX;
            dragStartRowPos = transform.position;
            velocity = 0f;
        }

        if (dragging && GetPressHeld(out Vector2 movePos))
        {
            float wx = ScreenToWorldX(movePos);
            float delta = wx - dragStartWorldX;
            transform.position = new Vector3(dragStartRowPos.x + delta, dragStartRowPos.y, dragStartRowPos.z);

            velocity = (wx - lastWorldX) / Mathf.Max(Time.deltaTime, 0.0001f);
            lastWorldX = wx;
            lastScreenX = movePos.x;
        }

        if (dragging && GetPressUp())
        {
            dragging = false;

            // Use the screen-space direction for navigation.  A camera rotated 180
            // degrees reverses world X, but a left swipe should always mean "next".
            float movedSlots = Mathf.Abs(transform.position.x - dragStartRowPos.x) / spacing;
            bool swipedLeft = lastScreenX < dragStartScreenX;

            // ลากไกลพอ หรือ ปัดเร็ว (flick) = เปลี่ยนหน้า
            bool flick = Mathf.Abs(velocity) > flickSpeed;

            if (swipedLeft && (movedSlots > switchThreshold || flick))
                current = NextIndex();
            else if (!swipedLeft && (movedSlots > switchThreshold || flick))
                current = PreviousIndex();

            targetPosition = PositionForIndex(current);
        }
    }

    Vector3 PositionForIndex(int index) => homePosition + ScreenLeftDirection() * (spacing * index);

    Vector3 ScreenLeftDirection()
    {
        if (cam == null) cam = Camera.main;

        // This slider moves only along world X, so preserve that constraint while
        // choosing the X direction that appears as left to the active camera.
        float x = -cam.transform.right.x;
        return Mathf.Abs(x) < 0.001f ? Vector3.left : Vector3.right * Mathf.Sign(x);
    }

    float ScreenToWorldX(Vector2 screenPos)
    {
        if (cam == null) cam = Camera.main;
        float depth = Mathf.Abs(cam.transform.position.z - transform.position.z);
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
