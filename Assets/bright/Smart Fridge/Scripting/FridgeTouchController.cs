using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// ติดที่ตัวโมเดลตู้เย็น (ต้องมี Collider)
[RequireComponent(typeof(Collider))]
public class FridgeTouchController : MonoBehaviour
{
    [Header("Animation (จะใส่หรือไม่ก็ได้)")]
    public Animator animator;
    public string openBoolParam = "IsOpen";

    [Header("UI Panel")]
    public GameObject uiPanel;

    [Header("Event ตอนเปิดตู้เย็นครั้งแรก (สุ่มของหมดครั้งเดียว)")]
    public UnityEvent onFirstOpen;   // ลาก SelectionDemo.BuildIngredients มาใส่ที่นี่

    private Camera cam;
    private bool isOpen;
    private bool hasOpenedBefore = false; // เคยเปิดแล้วหรือยัง

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
        if (isOpen) return; // UI เปิดอยู่ ไม่รับแตะตู้เย็น

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                if (IsPointerOverUI(t.fingerId)) return;
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

    public void OpenFridge()
    {
        isOpen = true;
        ApplyState();
        if (uiPanel != null) uiPanel.SetActive(true);

        // สุ่มของหมด "ครั้งเดียว" ตอนเปิดครั้งแรกเท่านั้น
        if (!hasOpenedBefore)
        {
            hasOpenedBefore = true;
            onFirstOpen?.Invoke();
        }
    }

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