using System.Collections.Generic;
using UnityEngine;

// เลื่อนสไลด์ซ้าย/ขวา สลับสเตชัน (หั่น/ทอด/ต้ม) แบบเลื่อนตามนิ้วแล้ว snap เข้าที่
// วิธีคิด: วางสเตชันเรียงต่อกันแนวนอน แล้วเลื่อน "แถวทั้งหมด" (ตัวนี้ควรเป็นแม่ของทุกสเตชัน)
public class SwipeStationSlider : MonoBehaviour
{
    [Header("ระยะห่างระหว่างสเตชัน (world units)")]
    [Tooltip("แต่ละสเตชันวางห่างกันเท่าไรตามแกน X เช่น 20")]
    public float spacing = 20f;

    [Header("จำนวนสเตชัน")]
    public int stationCount = 3;
    public int startIndex = 0;

    [Header("ความลื่น")]
    [Tooltip("ความเร็ว snap เข้าที่ (มาก = เร็ว)")]
    public float snapSpeed = 8f;
    [Tooltip("ลากไกลเกินเศษนี้ของ 1 ช่อง ถึงจะเปลี่ยนสเตชัน (0.5 = ครึ่งทาง)")]
    public float switchThreshold = 0.25f;

    [Header("เปิด/ปิดการเลื่อน (ปิดตอนหั่นเพื่อไม่ชนกับลากมีด)")]
    public bool swipeEnabled = true;

    private int current;
    private Vector3 homePosition;   // ตำแหน่งเริ่มของแถว (สเตชันแรกอยู่กลางจอ)
    private Vector3 targetPosition;
    private Camera cam;

    private bool dragging;
    private float dragStartWorldX;
    private Vector3 dragStartRowPos;

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

        // เลื่อนเข้าหาเป้าหมายแบบลื่น ๆ
        if (!dragging)
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * snapSpeed);
    }

    void HandleDrag()
    {
        // เริ่มลาก
        if (GetPressDown(out Vector2 downPos))
        {
            dragging = true;
            dragStartWorldX = ScreenToWorldX(downPos);
            dragStartRowPos = transform.position;
        }

        // ระหว่างลาก — แถวเลื่อนตามนิ้ว
        if (dragging && GetPressHeld(out Vector2 movePos))
        {
            float currentWorldX = ScreenToWorldX(movePos);
            float deltaX = currentWorldX - dragStartWorldX;
            transform.position = new Vector3(dragStartRowPos.x + deltaX, dragStartRowPos.y, dragStartRowPos.z);
        }

        // ปล่อย — ตัดสินใจว่าเปลี่ยนสเตชันไหม แล้ว snap
        if (dragging && GetPressUp())
        {
            dragging = false;

            float movedSlots = (dragStartRowPos.x - transform.position.x) / spacing;

            if (movedSlots > switchThreshold) current = Mathf.Min(current + 1, stationCount - 1);   // ลากไปซ้าย -> ถัดไป
            else if (movedSlots < -switchThreshold) current = Mathf.Max(current - 1, 0);             // ลากไปขวา -> ย้อนกลับ

            targetPosition = PositionForIndex(current);
        }
    }

    // ตำแหน่งของแถวเมื่ออยากให้สเตชัน index อยู่กลาง
    Vector3 PositionForIndex(int index)
    {
        return homePosition + Vector3.left * (spacing * index);
    }

    float ScreenToWorldX(Vector2 screenPos)
    {
        if (cam == null) cam = Camera.main;
        float depth = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, depth));
        return world.x;
    }

    // ปุ่มลูกศร (เผื่ออยากมีปุ่มด้วย)
    public void Next()     { current = Mathf.Min(current + 1, stationCount - 1); targetPosition = PositionForIndex(current); }
    public void Previous() { current = Mathf.Max(current - 1, 0);                targetPosition = PositionForIndex(current); }
    public void SetSwipeEnabled(bool v) => swipeEnabled = v;

    // ---------- อ่าน input (รองรับ touch + mouse) ----------
    bool GetPressDown(out Vector2 pos)
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        { pos = Input.GetTouch(0).position; return true; }
        if (Input.touchCount == 0 && Input.GetMouseButtonDown(0))
        { pos = Input.mousePosition; return true; }
        pos = Vector2.zero; return false;
    }

    bool GetPressHeld(out Vector2 pos)
    {
        if (Input.touchCount > 0)
        { pos = Input.GetTouch(0).position; return true; }
        if (Input.GetMouseButton(0))
        { pos = Input.mousePosition; return true; }
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