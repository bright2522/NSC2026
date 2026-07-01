using UnityEngine;

public class SausageCupPourController : MonoBehaviour
{
    private bool isDragging = false;
    private bool isAbovePan = false;
    private bool isPoured = false; 
    private bool isSnapping = false; 
    private bool isReturningToStart = false; 
    
    private Camera mainCamera;
    private Vector3 offset;
    private float zCoord;
    private Vector3 targetSnapPosition; 

    private Vector3 initialPosition;    
    private Quaternion initialRotation; 
    private Collider cupCollider;

    [Header("Settings")]
    public float tiltSpeed = 400.0f; 
    public float snapSpeed = 8.0f;     

    [Header("ระบบกำหนดองศาปล่อยไส้กรอก")]
    public float pourAngleThreshold = 120.0f; 

    [Header("ระบบล็อกอิสระ & แรงดูด")]
    public Transform lockTarget;       
    public float snapDistance = 3.0f;  

    [Header("🌭 กลุ่มไส้กรอกในถ้วย")]
    public SausageItemController[] sausagesInCup; 

    private float currentRotationZ = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        cupCollider = GetComponent<Collider>();

        SetSausagesKinematic(true);
    }

    void Update()
    {
        HandleDrag();

        if (isSnapping && lockTarget != null)
        {
            transform.position = Vector3.Lerp(transform.position, targetSnapPosition, Time.deltaTime * snapSpeed);

            if (Vector3.Distance(transform.position, targetSnapPosition) < 0.05f)
            {
                transform.position = targetSnapPosition;
                isSnapping = false;
                isAbovePan = true;
                PanDragCoordinator.End(this);
                currentRotationZ = 0f;
                transform.rotation = Quaternion.identity;
            }
        }

        if (isReturningToStart)
        {
            transform.position = Vector3.Lerp(transform.position, initialPosition, Time.deltaTime * snapSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, initialRotation, Time.deltaTime * snapSpeed);

            if (Vector3.Distance(transform.position, initialPosition) < 0.05f)
            {
                transform.position = initialPosition;
                transform.rotation = initialRotation;
                isReturningToStart = false;
            }
        }

        if (isAbovePan && !isDragging && !isSnapping && !isPoured && !isReturningToStart)
        {
            SetCupColliderEnabled(false);
            HandleTilt();
        }
        else if (!isAbovePan || isDragging || isSnapping || isPoured || isReturningToStart)
        {
            SetCupColliderEnabled(!isPoured && !isReturningToStart);
        }
    }

    void HandleDrag()
    {
        if (isPoured || isReturningToStart) return;

        if (isDragging || isSnapping)
        {
            PanDragCoordinator.Maintain(this);

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
                    }
                    else
                    {
                        PanDragCoordinator.End(this);
                    }
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
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && PanDragCoordinator.IsHitOnObject(hit, transform))
            {
                if (PanDragCoordinator.TryBegin(this))
                {
                    isDragging = true;
                    isAbovePan = false;
                    isSnapping = false;
                    zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
                    offset = transform.position - GetMouseWorldPos();
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

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            inputX = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            inputX = 1f;
#else
        inputX = Input.acceleration.x;
#endif

        if (inputX < 0)
        {
            currentRotationZ -= inputX * tiltSpeed * Time.deltaTime;
        }
        else
        {
            currentRotationZ = Mathf.MoveTowards(currentRotationZ, 0f, tiltSpeed * 0.75f * Time.deltaTime);
        }

        currentRotationZ = Mathf.Clamp(currentRotationZ, 0f, 120f);
        transform.localRotation = Quaternion.Euler(0f, 0f, currentRotationZ);

        if (currentRotationZ >= pourAngleThreshold && !isPoured)
        {
            PourSausages();
        }
    }

    void PourSausages()
    {
        isPoured = true;
        
        DetachAndDropSausages();

        if (PanPrepManager.Instance != null)
        {
            PanPrepManager.Instance.MarkSausageDone();
        }

        StartCoroutine(ReturnRoutine());
    }

    void DetachAndDropSausages()
    {
        foreach (SausageItemController sausage in sausagesInCup)
        {
            if (sausage != null)
            {
                // 1. ตัดขาดความสัมพันธ์แม่ลูกออกจากถ้วยไปอยู่รากแกนโลก
                sausage.transform.SetParent(null, true);

                // 2. ปลดล็อก Rigidbody ฟิสิกส์ให้ทำงาน
                Rigidbody rb = sausage.GetComponent<Rigidbody>();
                if (rb == null) rb = sausage.GetComponentInChildren<Rigidbody>();
                
                if (rb != null)
                {
                    rb.isKinematic = false; 
                    rb.useGravity = true;   
                }
            }
        }
    }

    System.Collections.IEnumerator ReturnRoutine()
    {
        // ⏳ 🔥 แก้ไขตรงนี้: หน่วงเวลาค้างไว้ 2 วินาทีเต็มให้ไส้กรอกหล่นเคลียร์จนเสร็จ
        yield return new WaitForSeconds(2.0f);
        
        isAbovePan = false;
        isReturningToStart = true; // ค่อยสั่งถ้วยเปล่าวิ่งกลับไปที่เดิม
    }

    void SetCupColliderEnabled(bool enabled)
    {
        if (cupCollider != null)
        {
            cupCollider.enabled = enabled;
        }
    }

    void SetSausagesKinematic(bool state)
    {
        foreach (SausageItemController sausage in sausagesInCup)
        {
            if (sausage != null)
            {
                Rigidbody rb = sausage.GetComponent<Rigidbody>();
                if (rb == null) rb = sausage.GetComponentInChildren<Rigidbody>();
                
                if (rb != null)
                {
                    rb.isKinematic = state;
                    rb.useGravity = !state;
                }
            }
        }
    }
}