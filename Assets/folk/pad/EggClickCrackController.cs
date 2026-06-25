using UnityEngine;
using System.Collections;

public class EggClickCrackController : MonoBehaviour
{
    private bool isDragging = false;
    private bool isLockedInPlace = false; 

    private Camera mainCamera;
    private Vector3 offset;
    private float zCoord;
    private Vector3 initialPosition;

    [Header("ระบบล็อกตำแหน่งเหนือกระทะ")]
    public Transform panLockTarget;
    public float snapDistance = 3.0f;
    public float snapSpeed = 10.0f;
    public Vector3 lockedRotationTarget = new Vector3(0f, 0f, 90f);

    [Header("โมเดลไข่กลมตั้งต้น")]
    public GameObject fullEggVisual;

    [Header("โมเดลเปลือกไข่ฝั่งซ้ายและขวา")]
    public GameObject shellLeftPrefab;
    public GameObject shellRightPrefab;

    [Header("ระยะห่างการแยกของเปลือกไข่")]
    public float shellSeparateDistance = 1.5f; 

    [Header("ตั้งค่าขนาดสเกลของเปลือกไข่ซ้าย-ขวาตอนแตกออก")]
    public Vector3 customShellLeftScale = new Vector3(1f, 1f, 1f);
    public Vector3 customShellRightScale = new Vector3(1f, 1f, 1f);

    [Header("โมเดลไข่ดาวรวมชิ้นสำเร็จรูป")]
    public GameObject friedEggPrefab;
    public Transform spawnPoint;

    [Header("ชื่อของ Object ไข่ขาวในโมเดล (พิมพ์ให้ตรงตัวพิมพ์เล็ก-ใหญ่)")]
    public string eggWhiteObjectName = "EggWhite"; 

    [Header("รูปทรงไข่แดงตอนร่วงหล่น (กลมๆ หนาๆ เหมือนหยดน้ำ)")]
    public Vector3 eggFallingScale = new Vector3(0.6f, 1.2f, 0.6f); 

    [Header("รูปทรงตอนแผ่แบนราบสำเร็จรูป (เมื่อถึงพื้นกระทะแล้ว)")]
    public Vector3 eggFlattenedScale = new Vector3(1f, 1f, 1f);

    [Header("ความเร็วในการแผ่ตัวแบนราบตอนถึงกระทะ")]
    public float flattenSpeed = 5f;

    // 🎯 [คุณสมบัติใหม่] จุดกึ่งกลางกระทะที่ต้องการให้ไข่วาร์ปไปลงล็อก
    [Header("🎯 วัตถุเป้าหมายกึ่งกลางกระทะ (Center of Pan)")]
    public Transform panCenterTarget;

    void Start()
    {
        mainCamera = Camera.main;
        initialPosition = transform.position;
    }

    void Update()
    {
        if (isLockedInPlace && panLockTarget != null)
        {
            transform.position = Vector3.Lerp(transform.position, panLockTarget.position, Time.deltaTime * snapSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(lockedRotationTarget), Time.deltaTime * snapSpeed);
            
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform == this.transform)
                    {
                        CrackEgg();
                    }
                }
            }
        }
        else if (!isDragging && transform.position != initialPosition)
        {
            transform.position = Vector3.Lerp(transform.position, initialPosition, Time.deltaTime * 5f);
        }
    }

    void OnMouseDown()
    {
        if (isLockedInPlace) return;
        isDragging = true;
        zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        if (isDragging && !isLockedInPlace)
        {
            Vector3 targetPos = GetMouseWorldPos() + offset;
            transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        }
    }

    void OnMouseUp()
    {
        if (isLockedInPlace) return;
        isDragging = false;

        if (panLockTarget != null)
        {
            float distance = Vector3.Distance(transform.position, panLockTarget.position);
            if (distance <= snapDistance)
            {
                isLockedInPlace = true;
                Debug.Log("🔒 ไข่เข้าล็อกพิกัดแล้ว! คลิกตอกได้เลย");
            }
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return mainCamera.ScreenPointToRay(mousePoint).GetPoint(zCoord);
    }

    void CrackEgg()
    {
        Debug.Log("💥 ตอกไข่ดิบและเตรียมระบบดูดเข้าหาจุดกึ่งกลางกระทะ");

        Vector3 leftSpawnPos = transform.position + (Vector3.left * shellSeparateDistance);
        Vector3 rightSpawnPos = transform.position + (Vector3.right * shellSeparateDistance);

        GameObject leftShell = null;
        GameObject rightShell = null;

        if (shellLeftPrefab != null)
        {
            leftShell = Instantiate(shellLeftPrefab, leftSpawnPos, Quaternion.Euler(-90f, 0f, 0f));
            leftShell.transform.localScale = customShellLeftScale;
        }

        if (shellRightPrefab != null)
        {
            rightShell = Instantiate(shellRightPrefab, rightSpawnPos, Quaternion.Euler(-90f, 0f, 0f));
            rightShell.transform.localScale = customShellRightScale;
        }

        if (leftShell != null) leftShell.AddComponent<ShellClickDestroy>().SetupPairs(leftShell, rightShell);
        if (rightShell != null) rightShell.AddComponent<ShellClickDestroy>().SetupPairs(leftShell, rightShell);

        Vector3 spawnPos = (spawnPoint != null) ? spawnPoint.position : transform.position;

        if (friedEggPrefab != null)
        {
            GameObject friedEgg = Instantiate(friedEggPrefab, spawnPos, friedEggPrefab.transform.rotation);
            friedEgg.transform.localScale = eggFallingScale; 

            Rigidbody eggRb = friedEgg.GetComponent<Rigidbody>();
            if (eggRb == null) eggRb = friedEgg.AddComponent<Rigidbody>();
            
            eggRb.isKinematic = false;
            eggRb.useGravity = true;

            Collider eggCollider = friedEgg.GetComponent<Collider>();
            if (eggCollider != null)
            {
                if (leftShell != null) { Collider c = leftShell.GetComponent<Collider>(); if (c != null) Physics.IgnoreCollision(eggCollider, c); }
                if (rightShell != null) { Collider c = rightShell.GetComponent<Collider>(); if (c != null) Physics.IgnoreCollision(eggCollider, c); }
            }

            // 🎯 ส่งค่าเป้าหมายกึ่งกลางกระทะ (panCenterTarget) เข้าสคริปต์ผู้ช่วยแผ่ไข่
            friedEgg.AddComponent<EggFlattenEffect>().Setup(eggFlattenedScale, flattenSpeed, eggWhiteObjectName, panCenterTarget);
        }

        if (StirFryManager.Instance != null)
        {
            StirFryManager.Instance.ResetProgress();
        }

        Destroy(gameObject);
    }
}

public class ShellClickDestroy : MonoBehaviour
{
    private GameObject partner1;
    private GameObject partner2;

    public void SetupPairs(GameObject p1, GameObject p2)
    {
        partner1 = p1;
        partner2 = p2;

        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) box = gameObject.AddComponent<BoxCollider>();
        if (box != null)
        {
            box.center = Vector3.zero;
            box.size = new Vector3(0.3f, 0.3f, 0.3f); 
        }
    }

    void OnMouseDown()
    {
        if (partner1 != null) Destroy(partner1);
        if (partner2 != null) Destroy(partner2);
    }
}

// 🎯 สคริปต์ผู้ช่วย: แผ่ไข่แบน + ดูดสไลด์เข้าหาเป้าหมายกึ่งกลางกระทะ
public class EggFlattenEffect : MonoBehaviour
{
    private Vector3 targetFlattenScale;
    private float speed;
    private string whiteName;
    private Transform eggWhiteTransform;
    private Transform centerTarget;
    private bool hasHitPan = false;

    public void Setup(Vector3 finalScale, float lerpSpeed, string whiteObjName, Transform panCenter)
    {
        targetFlattenScale = finalScale;
        speed = lerpSpeed;
        whiteName = whiteObjName;
        centerTarget = panCenter; // รับค่าจุดดูดศูนย์กลาง

        Transform child = transform.Find(whiteName);
        if (child != null)
        {
            eggWhiteTransform = child;
            eggWhiteTransform.gameObject.SetActive(false); 
        }
        else
        {
            foreach (Transform t in transform)
            {
                if (t.name.Contains("White") || t.name.Contains("white") || t.name == whiteName)
                {
                    eggWhiteTransform = t;
                    eggWhiteTransform.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasHitPan)
        {
            hasHitPan = true;

            // 🎯 สั่งปิดแรงฟิสิกส์กระดอนเพื่อไม่ให้มันส่ายหนี และเตรียมตัวสไลด์เข้าจุดศูนย์กลาง
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; 
            }

            if (eggWhiteTransform != null) eggWhiteTransform.gameObject.SetActive(true);
            StartCoroutine(FlattenAndSnapRoutine());
        }
    }

    IEnumerator FlattenAndSnapRoutine()
    {
        float t = 0;
        Vector3 startScale = transform.localScale;
        Vector3 startPosition = transform.position;

        // ดึงค่าตำแหน่งเป้าหมาย (หากไม่มีเป้าหมายผูกไว้จะอยู่ที่พิกัดเดิม)
        Vector3 targetPosition = (centerTarget != null) ? centerTarget.position : startPosition;
        
        // บังคับให้รักษาความสูงแกน Y ตอนแตะก้นกระทะไว้ เพื่อไม่ให้ไข่จมดินหรือลอยฟ้า
        targetPosition.y = startPosition.y; 

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            
            // 🎯 ค่อยๆ ขยายตัวแผ่แบน
            transform.localScale = Vector3.Lerp(startScale, targetFlattenScale, t);
            
            // 🎯 ควบคู่กับค่อยๆ ดูดและสไลด์ตำแหน่งไข่ดาวเข้าหาพิกัดเป้าหมายแบบสมูท
            if (centerTarget != null)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            }

            yield return null;
        }
    }
}