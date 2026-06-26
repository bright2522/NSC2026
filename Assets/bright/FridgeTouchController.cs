using UnityEngine;

// ติดสคริปต์นี้ไว้ที่ตัวโมเดลตู้เย็น (ตัวที่มี Collider)
// ทำหน้าที่: แตะจอ -> เล่นอนิเมชั่นเปิดประตู / แตะอีกที -> ปิด
[RequireComponent(typeof(Collider))]
public class FridgeTouchController : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;              // ลาก Animator ของตู้เย็นมาใส่
    public string openBoolParam = "IsOpen"; // ชื่อ bool parameter ใน Animator

    [Header("Options")]
    public bool startOpen = false;          // เริ่มมาเปิดอยู่ไหม

    private Camera cam;
    private bool isOpen;

    void Start()
    {
        cam = Camera.main;                  // ใช้กล้องหลัก (ต้องมี tag = MainCamera)
        if (animator == null) animator = GetComponent<Animator>();

        isOpen = startOpen;
        ApplyState();
    }

    void Update()
    {
        // --- รองรับจอสัมผัส (มือถือ/touch screen) ---
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
                TryTouch(t.position);
        }
        // --- รองรับเมาส์ (ไว้ทดสอบใน Editor) ---
        else if (Input.GetMouseButtonDown(0))
        {
            TryTouch(Input.mousePosition);
        }
    }

    // ยิงรังสีจากจุดที่แตะ เช็คว่าโดนตู้เย็นไหม
    void TryTouch(Vector2 screenPos)
    {
        if (cam == null) cam = Camera.main;

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // โดนตัวนี้ หรือลูก ๆ ของตู้เย็น (เช่นบานประตู) ก็นับว่าโดน
            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
            {
                Toggle();
            }
        }
    }

    // สลับเปิด/ปิด — เรียกจากปุ่ม UI ก็ได้ (ลากใส่ช่อง OnClick)
    public void Toggle()
    {
        SetOpen(!isOpen);
    }

    public void SetOpen(bool open)
    {
        isOpen = open;
        ApplyState();
    }

    void ApplyState()
    {
        if (animator != null)
            animator.SetBool(openBoolParam, isOpen);
    }
}