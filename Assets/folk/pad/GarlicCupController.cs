using UnityEngine;

public class GarlicCupController : MonoBehaviour
{
    private bool isDragging = false;
    private bool isAbovePan = false;
    private bool isGarlicReleased = false; 
    private bool isSnapping = false; 
    
    private Camera mainCamera;
    private Vector3 offset;
    private float zCoord;
    private Vector3 targetSnapPosition; 

    [Header("Settings")]
    public float tiltSpeed = 150.0f;     
    public float snapSpeed = 8.0f;     

    [Header("ระบบกำหนดองศาปล่อยกระเทียม")]
    [Tooltip("องศาการเอียงถ้วยขั้นต่ำ (แกน Z) ที่จะเริ่มปล่อยให้กระเทียมไหลออก")]
    public float minReleaseAngle = 25.0f; 

    [Header("ระบบล็อกอิสระ & แรงดูด")]
    public Transform lockTarget;       
    public float snapDistance = 3.0f;  

    [Header("ระบบเสกกระเทียม (Spawning)")]
    public GameObject garlicPrefab;    
    public Transform spawnPoint;       
    public int garlicAmount = 100;       
    [Tooltip("ความถี่ในการไหล ยิ่งน้อยกระเทียมยิ่งไหลพรูต่อเนื่อง")]
    public float spawnInterval = 0.03f; 

    // เก็บค่าองศาการเอียงแกน Z (ซ้าย-ขวา) อย่างเดียว
    private float currentRotationZ = 0f;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleDrag();

        // ระบบ Lerp สไลด์ถ้วยเข้าจุดล็อกอัตโนมัติ
        if (isSnapping && lockTarget != null)
        {
            transform.position = Vector3.Lerp(transform.position, targetSnapPosition, Time.deltaTime * snapSpeed);

            if (Vector3.Distance(transform.position, targetSnapPosition) < 0.05f)
            {
                transform.position = targetSnapPosition;
                isSnapping = false;
                isAbovePan = true; 
                
                currentRotationZ = 0f;
                transform.rotation = Quaternion.identity;
                Debug.Log("🔒 [STATUS] ถ้วยเข้าล็อกตำแหน่งถาวรแล้ว! พร้อมเอียงซ้าย");
            }
        }

        // ถ้าระบบดูดเข้าล็อกเรียบร้อยแล้ว และไม่ได้ลากอยู่ -> ให้เริ่มระบบเอียงถ้วย
        if (isAbovePan && !isDragging && !isSnapping)
        {
            HandleTilt();
        }
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit) && (hit.transform == transform || hit.transform.IsChildOf(transform)))
            {
                isDragging = true;
                isAbovePan = false; 
                isSnapping = false; 
                zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
                offset = transform.position - GetMouseWorldPos();
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 targetPos = GetMouseWorldPos() + offset;
            transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            
            if (lockTarget != null)
            {
                Vector2 cupPos2D = new Vector2(transform.position.x, transform.position.y);
                Vector2 targetPos2D = new Vector2(lockTarget.position.x, lockTarget.position.y);
                
                float finalDistance = Vector2.Distance(cupPos2D, targetPos2D);
                
                if (finalDistance < snapDistance) 
                {
                    isSnapping = true;
                    targetSnapPosition = new Vector3(lockTarget.position.x, lockTarget.position.y, transform.position.z);
                    Debug.Log("🧲 [MAGNET] ปล่อยมือแล้ว! กำลังดูดสไลด์เข้าจุดล็อก...");
                }
            }
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return mainCamera.ScreenPointToRay(mousePoint).GetPoint(zCoord);
    }

    void HandleTilt()
    {
        float inputX = 0f;

        // 1. รองรับการเอียงข้อมือบนมือถือ (ระบบ Gyro / Accelerometer)
        Vector3 acceleration = Input.acceleration;
        if (acceleration != Vector3.zero)
        {
            inputX = acceleration.x;
        }

        // 2. รองรับคีย์บอร์ดบนคอมพิวเตอร์ (ปุ่ม A หรือ ลูกศรซ้าย เพื่อเอียงซ้าย)
        if (inputX == 0)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputX = -1f;
        }

        // เช็คเงื่อนไขให้ตอบสนอง "เฉพาะตอนเอียงซ้าย" (ปุ่ม A หรือ เอียงเครื่องซ้าย ค่าจะเป็นลบ)
        if (inputX < 0)
        {
            currentRotationZ -= inputX * tiltSpeed * Time.deltaTime;
        }
        else
        {
            // ถ้าปล่อยปุ่มหรือตั้งเครื่องตรง ให้ถ้วยค่อยๆ คืนองศากลับมาหน้าตรงอัตโนมัติ
            currentRotationZ = Mathf.MoveTowards(currentRotationZ, 0f, tiltSpeed * 0.75f * Time.deltaTime);
        }

        // ล็อกมุมเอียง: ต่ำสุดคือ 0 (หน้าตรง) และสูงสุดคือ 60 องศา (เอียงซ้าย) ห้ามเอียงไปทางขวาเด็ดขาด
        currentRotationZ = Mathf.Clamp(currentRotationZ, 0f, 60f);

        // หมุนถ้วยเฉพาะแกน Z เท่านั้น
        transform.localRotation = Quaternion.Euler(0f, 0f, currentRotationZ);

        // 🔥 ตรวจสอบองศาที่กำหนดไว้ใน Inspector ก่อนทำการปล่อยกระเทียม
        if (currentRotationZ > minReleaseAngle && !isGarlicReleased)
        {
            isGarlicReleased = true;
            StartCoroutine(SpawnGarlicRoutine());
        }
    }

    System.Collections.IEnumerator SpawnGarlicRoutine()
    {
        Debug.Log($"🎯 [FLOW] ถ้วยเอียงเกิน {minReleaseAngle} องศาแล้ว! กระเทียมกำลังไหลลงตามแรงโน้มถ่วงธรรมชาติ...");
        
        for (int i = 0; i < garlicAmount; i++)
        {
            if (garlicPrefab != null && spawnPoint != null)
            {
                // กระจายจุดเกิดเล็กน้อยในกรอบปากถ้วย เพื่อป้องกันไม่ให้โมเดลทับที่กันจนฟิสิกส์ดีดเด้ง
                float randomX = Random.Range(-0.06f, 0.06f);
                float randomY = Random.Range(-0.03f, 0.03f);
                float randomZ = Random.Range(-0.06f, 0.06f);
                Vector3 spawnOffset = (spawnPoint.right * randomX) + (spawnPoint.up * randomY) + (spawnPoint.forward * randomZ);

                // สร้างกระเทียมพร้อมปล่อยร่วงตามแรงโน้มถ่วงทันที
                GameObject spawnedGarlic = Instantiate(garlicPrefab, spawnPoint.position + spawnOffset, Random.rotation);
                
                Rigidbody rb = spawnedGarlic.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // ล้างความเร็วตกค้างเก่าออก ป้องกันอาการเด้งตั้งแต่เฟรมแรก
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
            // ปล่อยให้เวลาหน่วงเล็กน้อยเพื่อให้กระเทียมไหลออกมาเป็นสายธรรมชาติ ไม่กองเป็นก้อนเดียว
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}