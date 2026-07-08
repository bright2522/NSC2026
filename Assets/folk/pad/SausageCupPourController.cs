using UnityEngine;

public class SausageCupPourController : MonoBehaviour
{
    private bool isDragging = false;
    private bool isAbovePan = false;
    private bool isPoured = false; 
    private bool isSnapping = false; 
    private bool isReturningToStart = false; 
    
    // 🎥 เปลี่ยนตัวแปรหลักมาใช้ตัวที่กำหนดเองได้ ไม่พึ่งพาแค่ Camera.main ส่วนกลางอย่างเดียว
    private Camera activeCamera;
    private Vector3 offset;
    private float zCoord;
    private Vector3 targetSnapPosition; 

    private Vector3 initialPosition;    
    private Quaternion initialRotation; 
    private Collider cupCollider;

    [Header("ระบบล็อกกล้องเฉพาะตัว (ป้องกันเอ๋อเมื่อมีกล้องหลายตัว)")]
    [Tooltip("ลากกล้องตัวที่ 3 (กล้องที่ใช้คุมการทำอาหารในซีนนี้) มาหย่อนใส่ช่องนี้ได้เลย")]
    public Camera customCamera; 

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
        CupTiltInput.EnableSensors();

        // 🛠️ ลอจิกเลือกกล้อง: ถ้าลากกล้องตัวที่ 3 มาใส่ใน Inspector ให้ใช้ตัวนั้นทันที 
        // แต่ถ้าไม่ได้ลากใส่ (เป็นค่าว่าง) ให้มันถอยกลับไปใช้ Camera.main ดั้งเดิมเพื่อความเซฟตี้
        if (customCamera != null)
        {
            activeCamera = customCamera;
        }
        else
        {
            activeCamera = Camera.main;
            Debug.LogWarning($"[SausageCup] ไม่พบการใส่กล้องในช่อง Custom Camera จึงสลับไปใช้ Camera.main ส่วนกลาง");
        }

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
                CupTiltInput.CalibrateNeutral();
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
            // 🎯 เปลี่ยนมาทำ Raycast ผ่านตัวแปร activeCamera (ซึ่งผูกกับกล้องตัวที่ 3 แล้ว)
            Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && PanDragCoordinator.IsHitOnObject(hit, transform))
            {
                if (PanDragCoordinator.TryBegin(this))
                {
                    isDragging = true;
                    isAbovePan = false;
                    isSnapping = false;
                    zCoord = activeCamera.WorldToScreenPoint(transform.position).z;
                    offset = transform.position - GetMouseWorldPos();
                }
            }
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        // 🎯 เปลี่ยนมาแปลงพิกัดหน้าจอสู่พิกัดโลกด้วยกล้องที่เราเลือกโดยตรง
        return activeCamera.ScreenPointToRay(mousePoint).GetPoint(zCoord);
    }

    void HandleTilt()
    {
        float inputX = CupTiltInput.ReadPourAxis();

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

        GameplayScore.Instance?.AddScore(30);

        StartCoroutine(ReturnRoutine());
    }

    void DetachAndDropSausages()
    {
        foreach (SausageItemController sausage in sausagesInCup)
        {
            if (sausage != null)
            {
                sausage.transform.SetParent(null, true);

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
        yield return new WaitForSeconds(2.0f);
        
        isAbovePan = false;
        isReturningToStart = true; 
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