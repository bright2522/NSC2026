using UnityEngine;

public class BottleTilt : MonoBehaviour
{
    [Header("Settings")]
    public float tiltSpeed = 5f;
    public float maxTiltAngle = 90f;

    [Header("Control Flag")]
    // เปิดตัวแปรนี้ให้ BottleDrag สามารถเข้ามาเปิด/ปิดระบบเอียงได้
    public bool canTilt = false; 

    private Quaternion targetRotation;

    void Start()
    {
        targetRotation = transform.localRotation;
    }

    void Update()
    {
        if (canTilt)
        {
            HandleMobileTilt();
        }
        else
        {
            // ถ้ายังไม่เข้าจุด ให้ขวดตั้งตรงปกติ
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * tiltSpeed);
        }
    }

    void HandleMobileTilt()
    {
        // ดึงค่าการเอียงจากมือถือ
        float tiltInput = Input.acceleration.x;

        // ถ้าทดสอบบนคอมพิวเตอร์ (Editor) ให้ใช้ปุ่ม A, D หรือ ลูกศรซ้าย-ขวา แทน
        if (Application.isEditor && tiltInput == 0)
        {
            tiltInput = Input.GetAxis("Horizontal");
        }

        // คำนวณองศาแกน Z (สามารถเปลี่ยนเป็นแกน X ได้ตามทิศทางโมเดลขวดของคุณ)
        float targetZAngle = -tiltInput * maxTiltAngle;
        targetRotation = Quaternion.Euler(0, 0, targetZAngle);

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * tiltSpeed);
    }
}