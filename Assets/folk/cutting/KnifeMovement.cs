using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // ระบบใหม่ของ Unity 6

public class KnifeMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float topYPosition = 1.5f;    // ความสูงตอนยกเล็ง
    public float bottomYPosition = 0.15f; // ความสูงตอนสับลงสุด (ปรับให้เฉือนทะลุเขียงนิดหน่อย)
    public float chopSpeed = 40f;        // ความเร็วตอนสับ
    public float resetSpeed = 10f;       // ความเร็วตอนยกมีดกลับ

    private Camera mainCamera;
    private bool isDragging = false;
    private bool isChopping = false;
    private Plane dragPlane;             
    private Collider knifeCollider;

    void Start()
    {
        mainCamera = Camera.main;
        knifeCollider = GetComponent<Collider>();
        
        // บังคับให้มีดอยู่ในระดับยกเล็งตอนเริ่มเกม
        transform.position = new Vector3(transform.position.x, topYPosition, transform.position.z);
    }

    void Update()
    {
        if (isChopping) return;

        // 🌟 1. รวมดึงค่าพิกัดหน้าจอ (รองรับทั้ง นิ้วมือบนมือถือ และ เมาส์บน PC)
        Vector2 inputScreenPosition = GetInputPosition();

        // 🌟 2. จังหวะเริ่มกด/เริ่มแตะ (WasPressed)
        if (IsInputPressedThisFrame())
        {
            Ray ray = mainCamera.ScreenPointToRay(inputScreenPosition);
            RaycastHit hit;

            if (knifeCollider.Raycast(ray, out hit, Mathf.Infinity))
            {
                isDragging = true;
                dragPlane = new Plane(Vector3.up, new Vector3(0, topYPosition, 0));
            }
        }

        // 🌟 3. จังหวะลากมีดค้าง (IsHeld)
        if (isDragging && IsInputHeld())
        {
            Ray ray = mainCamera.ScreenPointToRay(inputScreenPosition);
            float enter = 0.0f;

            if (dragPlane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                // ล็อกแกน Y และ Z ให้คงที่ วิ่งตามนิ้ว/เมาส์แค่แกน X บนระนาบเล็ง
                transform.position = new Vector3(hitPoint.x, topYPosition, transform.position.z);
            }
        }

        // 🌟 4. จังหวะยกนิ้ว/ปล่อยเมาส์ -> สับทันที! (WasReleased)
        if (isDragging && IsInputReleasedThisFrame())
        {
            isDragging = false;
            StartCoroutine(ChopRoutine());
        }
    }

    // ฟังก์ชันช่วยดึงพิกัดหน้าจอแบบรองรับ 2 ฝั่ง
    private Vector2 GetInputPosition()
    {
        // เช็กถ้าเจอบนมือถือ (Touchscreen) ให้ดึงค่าจากนิ้วแรกก่อน
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            return Touchscreen.current.touches[0].position.ReadValue();
        }
        // ถ้าไม่เจอ (เล่นบนคอม) ให้ดึงจากเมาส์ปกติ
        else if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
        return Vector2.zero;
    }

    // ฟังก์ชันเช็กว่าเฟรมนี้เริ่มกดหรือยัง
    private bool IsInputPressedThisFrame()
    {
        bool mobileTouch = Touchscreen.current != null && Touchscreen.current.touches.Count > 0 && Touchscreen.current.touches[0].press.wasPressedThisFrame;
        bool mouseClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        return mobileTouch || mouseClick;
    }

    // ฟังก์ชันเช็กว่ายังกดลากค้างอยู่อิสระไหม
    private bool IsInputHeld()
    {
        bool mobileTouch = Touchscreen.current != null && Touchscreen.current.touches.Count > 0 && Touchscreen.current.touches[0].press.isPressed;
        bool mouseClick = Mouse.current != null && Mouse.current.leftButton.isPressed;
        return mobileTouch || mouseClick;
    }

    // ฟังก์ชันเช็กจังหวะยกนิ้วหรือปล่อยเมาส์
    private bool IsInputReleasedThisFrame()
    {
        bool mobileTouch = Touchscreen.current != null && Touchscreen.current.touches.Count > 0 && Touchscreen.current.touches[0].press.wasReleasedThisFrame;
        bool mouseClick = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
        return mobileTouch || mouseClick;
    }

    private IEnumerator ChopRoutine()
    {
        isChopping = true;

        // สับลงมาที่พื้นเขียง
        Vector3 targetPos = new Vector3(transform.position.x, bottomYPosition, transform.position.z);
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, chopSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        yield return new WaitForSeconds(0.04f); // ค้างมีดไว้แป๊บนึง เพิ่มฟีลสะใจแบบ ASMR

        // ยกมีดกลับขึ้นไปด้านบน
        Vector3 resetPos = new Vector3(transform.position.x, topYPosition, transform.position.z);
        while (Vector3.Distance(transform.position, resetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, resetPos, resetSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = resetPos;

        isChopping = false;
    }
}