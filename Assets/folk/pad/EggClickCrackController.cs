using UnityEngine;
using System.Collections;

public class EggClickCrackController : MonoBehaviour
{
    private bool isDragging = false;
    private bool isLockedInPlace = false; 

    private Camera activeCamera;
    private Vector3 offset;
    private float zCoord;
    private Vector3 initialPosition;

    [Header("ระบบล็อกกล้องเฉพาะตัว (ป้องกันเอ๋อเมื่อมีกล้องหลายตัว)")]
    public Camera customCamera;

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

    [Header("รูปทรงไข่แดงตอนร่วงหล่น")]
    public Vector3 eggFallingScale = new Vector3(0.6f, 1.2f, 0.6f); 

    [Header("รูปทรงตอนแผ่แบนราบสำเร็จรูป")]
    public Vector3 eggFlattenedScale = new Vector3(1f, 1f, 1f);

    [Header("ความเร็วในการแผ่ตัวแบนราบตอนถึงกระทะ")]
    public float flattenSpeed = 5f;

    [Header("🎯 วัตถุเป้าหมายกึ่งกลางกระทะ (Center of Pan)")]
    public Transform panCenterTarget;

    [Header("ชื่อของ Object ไข่ข้นในหน้า Hierarchy เพื่อสั่งล่องหนตอนเริ่มเกม")]
    public string scrambledEggObjectName = "New_Scrambled_Egg"; 

    void Start()
    {
        if (customCamera != null) activeCamera = customCamera;
        else activeCamera = Camera.main;

        initialPosition = transform.position;

        GameObject scrambledEggModel = GameObject.Find(scrambledEggObjectName);
        if (scrambledEggModel == null) scrambledEggModel = GameObject.Find("ScrambledEgg");

        if (scrambledEggModel != null)
        {
            Renderer scrambledRenderer = scrambledEggModel.GetComponentInChildren<Renderer>();
            if (scrambledRenderer != null && scrambledRenderer.material != null)
            {
                scrambledEggModel.SetActive(true);
                SetMaterialAlpha(scrambledRenderer.material, 0f);
            }
        }
    }

    void Update()
    {
        if (isLockedInPlace && panLockTarget != null)
        {
            PanDragCoordinator.Maintain(this);

            transform.position = Vector3.Lerp(transform.position, panLockTarget.position, Time.deltaTime * snapSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(lockedRotationTarget), Time.deltaTime * snapSpeed);
            
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.GetComponentInParent<ShellClickDestroy>() != null || hit.transform.CompareTag("Finish")) 
                    {
                        return; 
                    }

                    if (PanDragCoordinator.IsHitOnObject(hit, transform))
                    {
                        CrackEgg();
                    }
                }
            }
        }
        else
        {
            HandleDrag();

            if (!isDragging && transform.position != initialPosition)
            {
                transform.position = Vector3.Lerp(transform.position, initialPosition, Time.deltaTime * 5f);
            }
        }
    }

    void HandleDrag()
    {
        if (isLockedInPlace) return;

        if (isDragging)
        {
            PanDragCoordinator.Maintain(this);

            if (Input.GetMouseButton(0))
            {
                Vector3 targetPos = GetMouseWorldPos() + offset;
                transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;

                bool willLock = false;
                if (panLockTarget != null)
                {
                    float distance = Vector3.Distance(transform.position, panLockTarget.position);
                    willLock = distance <= snapDistance;
                }

                if (willLock)
                {
                    isLockedInPlace = true;
                    PanDragCoordinator.Maintain(this);
                }
                else
                {
                    PanDragCoordinator.End(this);
                }
            }

            return;
        }

        if (PanDragCoordinator.HasActiveInteraction) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.GetComponentInParent<ShellClickDestroy>() != null) return;

                if (PanDragCoordinator.IsHitOnObject(hit, transform))
                {
                    if (PanDragCoordinator.TryBegin(this))
                    {
                        isDragging = true;
                        zCoord = activeCamera.WorldToScreenPoint(transform.position).z;
                        offset = transform.position - GetMouseWorldPos();
                    }
                }
            }
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return activeCamera.ScreenPointToRay(mousePoint).GetPoint(zCoord);
    }

    void CrackEgg()
    {
        Collider parentCollider = GetComponent<Collider>();
        if (parentCollider != null) parentCollider.enabled = false;

        if (fullEggVisual != null) fullEggVisual.SetActive(false);

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

        if (leftShell != null) leftShell.AddComponent<ShellClickDestroy>().SetupPairs(leftShell, rightShell, activeCamera);
        if (rightShell != null) rightShell.AddComponent<ShellClickDestroy>().SetupPairs(leftShell, rightShell, activeCamera);

        Vector3 spawnPos = (spawnPoint != null) ? spawnPoint.position : transform.position;

        if (friedEggPrefab != null)
        {
            GameObject friedEgg = Instantiate(friedEggPrefab, spawnPos, friedEggPrefab.transform.rotation);
            friedEgg.transform.localScale = eggFallingScale; 

            Rigidbody eggRb = friedEgg.GetComponent<Rigidbody>();
            if (eggRb == null) eggRb = friedEgg.AddComponent<Rigidbody>();
            
            eggRb.isKinematic = false;
            eggRb.useGravity = true;
            eggRb.constraints = RigidbodyConstraints.FreezeRotation; 

            // สั่งเปิดระบบเช็คระยะทางจริงแทนการชน
            friedEgg.AddComponent<EggFlattenEffect>().Setup(eggFlattenedScale, flattenSpeed, eggWhiteObjectName, panCenterTarget);
        }

        if (StirFryManager.Instance != null) StirFryManager.Instance.ResetProgress();
        if (PanPrepManager.Instance != null) PanPrepManager.Instance.MarkEggDone();

        PanDragCoordinator.End(this);
        Destroy(gameObject);
    }

    void SetMaterialAlpha(Material mat, float alphaValue)
    {
        if (mat == null) return;
        if (mat.HasProperty("_Color"))
        {
            Color c = mat.color; c.a = alphaValue; mat.color = c;
        }
        else if (mat.HasProperty("_BaseColor"))
        {
            Color c = mat.GetColor("_BaseColor"); c.a = alphaValue; mat.SetColor("_BaseColor", c);
        }
    }
}

// ==========================================
// สคริปต์ผู้ช่วยชุดที่ 1: เปลือกไข่ลอยค้าง
// ==========================================
public class ShellClickDestroy : MonoBehaviour
{
    private GameObject partner1;
    private GameObject partner2;
    private Camera activeCamera; 

    public void SetupPairs(GameObject p1, GameObject p2, Camera gameCamera)
    {
        partner1 = p1;
        partner2 = p2;
        activeCamera = gameCamera; 

        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) box = gameObject.AddComponent<BoxCollider>();
        
        if (box != null)
        {
            box.enabled = true;
            box.center = Vector3.zero;
            box.size = new Vector3(0.6f, 0.6f, 0.6f); 
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void Start()
    {
        if (activeCamera == null)
        {
            EggClickCrackController parentController = FindFirstObjectByType<EggClickCrackController>();
            if (parentController != null && parentController.customCamera != null)
            {
                activeCamera = parentController.customCamera;
            }
            else
            {
                activeCamera = Camera.main;
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && activeCamera != null)
        {
            Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    if (partner1 != null) Destroy(partner1);
                    if (partner2 != null) Destroy(partner2);
                }
            }
        }
    }
}

// ==========================================
// 🛠️ [ยกเครื่องใหม่ทั้งหมด] สคริปต์ผู้ช่วยชุดที่ 2: ใช้ระบบ Check Distance ตัดปัญหาการชนบั๊ก
// ==========================================
public class EggFlattenEffect : MonoBehaviour
{
    private Vector3 targetFlattenScale;
    private float speed;
    private string whiteName;
    private Transform eggWhiteTransform;
    private Transform centerTarget;
    private bool hasHitPan = false;
    private Rigidbody rb;

    public void Setup(Vector3 finalScale, float lerpSpeed, string whiteObjName, Transform panCenter)
    {
        targetFlattenScale = finalScale;
        speed = lerpSpeed;
        whiteName = whiteObjName;
        centerTarget = panCenter;

        Transform child = transform.Find(whiteName);
        if (child != null)
        {
            eggWhiteTransform = child;
            eggWhiteTransform.gameObject.SetActive(true); 
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation; 
        }

        // 🎯 สั่งปิดทิ้งทุกฟิสิกส์คอลลิชันของตัวมันเอง ป้องกันปัญหาชนซ้อนเฟรมแรกกับเปลือกไข่เด็ดขาด!
        Collider c = GetComponent<Collider>();
        if (c != null) c.enabled = false;
        foreach (Collider childCollider in GetComponentsInChildren<Collider>())
        {
            childCollider.enabled = false;
        }
    }

    void Update()
    {
        // ถ้ายังลงไม่ถึงกระทะ ให้เช็คระยะห่างแกนดิ่ง (Y) ตลอดเวลา
        if (!hasHitPan && centerTarget != null)
        {
            // ถ้าความสูง (Y) ร่วงลงมาใกล้เคียง หรือ ต่ำกว่าพิกัดของก้นกระทะแล้ว ให้ทำงานทันที!
            if (transform.position.y <= centerTarget.position.y + 0.2f)
            {
                TriggerFlatten();
            }
        }
    }

    void TriggerFlatten()
    {
        hasHitPan = true;

        if (rb != null)
        {
            rb.isKinematic = true; 
            rb.useGravity = false;
        }

        StartCoroutine(FlattenAndSnapRoutine());
    }

    IEnumerator FlattenAndSnapRoutine()
    {
        float t = 0;
        Vector3 startScale = transform.localScale;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 targetPosition = (centerTarget != null) ? centerTarget.position : startPosition;
        Quaternion targetRotation = (centerTarget != null) ? centerTarget.rotation : Quaternion.identity;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            
            transform.localScale = Vector3.Lerp(startScale, targetFlattenScale, t);
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            
            if (centerTarget != null)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            }
            yield return null;
        }

        if (centerTarget != null)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation; 
        }
    }
}