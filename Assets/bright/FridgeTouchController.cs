using UnityEngine;
using UnityEngine.EventSystems;

// ติดที่ตัวโมเดลตู้เย็น (ต้องมี Collider)
// แตะตู้เย็น -> เปิดประตู + เด้ง UI / ปิด UI ด้วยปุ่ม X บนแผง
[RequireComponent(typeof(Collider))]
public class FridgeTouchController : MonoBehaviour
{
    [Header("Animation (จะใส่หรือไม่ก็ได้)")]
    public Animator animator;
    public string openBoolParam = "IsOpen";

    [Header("UI Panel")]
    public GameObject uiPanel;   // ลากแผง UI เลือกวัตถุดิบมาใส่

    private Camera cam;
    private bool isOpen;

    void Start()
    {
        cam = Camera.main;
        if (animator == null) animator = GetComponent<Animator>();

        isOpen = false;
        ApplyState();
        if (uiPanel != null) uiPanel.SetActive(false);
    }

    void Update()
    {
        // *** หัวใจของการแก้ ***
        // ถ้า UI เปิดอยู่แล้ว ตู้เย็นจะไม่รับการแตะใด ๆ อีก
        // -> กดติ๊กบน UI ได้โดยไม่ไปโดนตู้เย็น (ปิดด้วยปุ่ม X แทน)
        if (isOpen) return;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                if (IsPointerOverUI(t.fingerId)) return; // กันเผื่อมี UI อื่นบังอยู่
                TryTouch(t.position);
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI(-1)) return;
            TryTouch(Input.mousePosition);
        }
    }

    bool IsPointerOverUI(int pointerId)
    {
        if (EventSystem.current == null) return false;
        return pointerId == -1
            ? EventSystem.current.IsPointerOverGameObject()
            : EventSystem.current.IsPointerOverGameObject(pointerId);
    }

    void TryTouch(Vector2 screenPos)
    {
        if (cam == null) cam = Camera.main;

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
            {
                OpenFridge();
            }
        }
    }

    // เปิดตู้เย็น + โชว์ UI
    public void OpenFridge()
    {
        isOpen = true;
        ApplyState();
        if (uiPanel != null) uiPanel.SetActive(true);
    }

    // ปิดตู้เย็น + ซ่อน UI  <-- ผูกกับปุ่ม X บนแผง
    public void ClosePanel()
    {
        isOpen = false;
        ApplyState();
        if (uiPanel != null) uiPanel.SetActive(false);
    }

    void ApplyState()
    {
        if (animator != null)
            animator.SetBool(openBoolParam, isOpen);
    }
}