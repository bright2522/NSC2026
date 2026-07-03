using UnityEngine;

public class BottleController : MonoBehaviour
{
    [Header("Drag & Snap Settings")]
    private bool isDragging = false;
    private bool isSnapped = false;
    private Transform snapTarget;
    private Camera mainCamera;
    private Vector3 offset;
    private float zCoord;

    [Header("Tilt Settings")]
    [SerializeField] private float tiltSpeed = 5f;
    [SerializeField] private float minTiltAngle = -60f; // องศาเอียงซ้ายสุด
    [SerializeField] private float maxTiltAngle = 0f;    // องศากลับมาตั้งตรง
    private float currentTiltZ = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        // บันทึกมุมเริ่มต้นของขวด (สมมติว่าเริ่มที่ 0)
        currentTiltZ = transform.localEulerAngles.z;
    }

    void Update()
    {
        if (isDragging && !isSnapped)
        {
            DragBottle();
        }
        else if (isSnapped)
        {
            HandlePhoneTilt();
        }
    }

    #region Drag & Snap Mechanics

    void OnMouseDown()
    {
        if (isSnapped) return; // ถ้า Snap แล้ว ห้ามลากอีก

        isDragging = true;
        zCoord = mainCamera.WorldToScreenPoint(gameObject.transform.position).z;
        offset = gameObject.transform.position - GetMouseWorldPos();
    }

    void OnMouseUp()
    {
        isDragging = false;

        // ถ้าปล่อยมือแล้วอยู่ใกล้จุด Snap ให้ทำการ Snap ทันที
        if (snapTarget != null)
        {
            SnapToPan();
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }

    private void DragBottle()
    {
        transform.position = GetMouseWorldPos() + offset;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบว่าเข้าใกล้จุด Snap หรือยัง
        if (other.CompareTag("SnapZone") && !isSnapped)
        {
            snapTarget = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SnapZone") && !isSnapped)
        {
            snapTarget = null;
        }
    }

    private void SnapToPan()
    {
        isSnapped = true;
        isDragging = false;
        
        // ดูดขวดเข้าหาจุด Snap ทั้งตำแหน่งและมุมตั้งตรง
        transform.position = snapTarget.position;
        transform.rotation = Quaternion.Euler(0, 0, 0); 
    }

    #endregion

    #region Phone Tilt Mechanics

    private void HandlePhoneTilt()
    {
        // ใช้ Input.acceleration ในการตรวจจับความเอียงของมือถือ (แกน X ของมือถือมักจะส่งผลต่อการเอียงซ้าย-ขวา)
        // ค่าของ low-pass filter หรือ Input.acceleration.x จะอยู่ระหว่าง -1 ถึง 1
        float tiltInput = Input.acceleration.x;

        // ถ้าเอียงมือถือไปทางซ้าย ค่า tiltInput จะเป็นลบ
        if (tiltInput < 0)
        {
            // คำนวณเป้าหมายองศา โดยแปลงจากแรงเอียง (-1 ถึง 0) ไปเป็นองศา (minTiltAngle ถึง maxTiltAngle)
            float targetAngle = Mathf.Lerp(maxTiltAngle, minTiltAngle, Mathf.Abs(tiltInput));
            
            // ค่อยๆ หมุนไปตามความเร็วที่กำหนด
            currentTiltZ = Mathf.MoveTowardsAngle(currentTiltZ, targetAngle, tiltSpeed * Time.deltaTime * 50f);
        }
        else
        {
            // ถ้าไม่เอียงซ้าย หรือเอียงขวา ให้ขวดกลับมาตั้งตรง
            currentTiltZ = Mathf.MoveTowardsAngle(currentTiltZ, maxTiltAngle, tiltSpeed * Time.deltaTime * 50f);
        }

        // ล็อคให้เอียงเฉพาะแกน Z เท่านั้น
        transform.localRotation = Quaternion.Euler(0, 0, currentTiltZ);
    }

    #endregion
}