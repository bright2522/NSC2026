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

    // เก็บเป็น local space เพื่อให้กลับที่เดิมถูกต้องแม้แถวสเตชัน (SwipeStationSlider) จะเลื่อนไปแล้ว
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
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

        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        cupCollider = GetComponent<Collider>();

        SetSausagesKinematic(true);
    }

    void Update()
    {
        HandleDrag();

        if (isSnapping && lockTarget != null)
        {
            // คำนวณจุด snap ใหม่ทุกเฟรม เผื่อกระทะ/แถวสเตชันขยับระหว่าง snap
            targetSnapPosition = AlignLaterallyWith(lockTarget.position);
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
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialLocalPosition, Time.deltaTime * snapSpeed);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, initialLocalRotation, Time.deltaTime * snapSpeed);

            if (Vector3.Distance(transform.localPosition, initialLocalPosition) < 0.05f)
            {
                transform.localPosition = initialLocalPosition;
                transform.localRotation = initialLocalRotation;
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
                // เลื่อนในระนาบกล้อง (คงระยะลึกตามแกน forward ของกล้อง) — ห้ามล็อกแกน z ตรง ๆ
                // เพราะกับกล้อง top-down แกนลึกคือ Y ถ้าล็อก z ถ้วยจะมุดลงพื้นแทนที่จะเลื่อนตามนิ้ว
                transform.position = GetMouseWorldPos() + offset;
            }

            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                isDragging = false;

                if (lockTarget != null)
                {
                    // วัดระยะเฉพาะในระนาบกล้อง (ตัดแกนลึกออก) ใช้ได้ทั้งกล้องหน้าตรงและ top-down
                    float finalDistance = LateralDistanceTo(lockTarget.position);

                    if (finalDistance < snapDistance)
                    {
                        isSnapping = true;
                        targetSnapPosition = AlignLaterallyWith(lockTarget.position);
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
            // กล้องอาจถูกสลับเปิด/ปิดระหว่าง step (MinigameFlowManager) — ถ้าตัวที่ถืออยู่ปิดไปแล้ว
            // ให้หาตัวที่ active จริงตอนนี้ ไม่งั้น raycast/แปลงพิกัดจะเพี้ยนหมด
            if (activeCamera == null || !activeCamera.isActiveAndEnabled)
                activeCamera = (customCamera != null && customCamera.isActiveAndEnabled) ? customCamera : Camera.main;
            if (activeCamera == null) return;

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
        // ScreenToWorldPoint คืออินเวิร์สที่แท้จริงของ WorldToScreenPoint (zCoord = ระยะตามแกน forward)
        // ต่างจาก ray.GetPoint(zCoord) ที่จะเพี้ยนเมื่อเมาส์อยู่ห่างกลางจอ (ray เฉียง ระยะไม่เท่า depth)
        return activeCamera.ScreenToWorldPoint(mousePoint);
    }

    // ── ตัวช่วยเรขาคณิตแบบไม่ผูกกับทิศกล้อง ─────────────────────────────────
    // ระยะห่างจากถ้วยถึงเป้า เฉพาะในระนาบตั้งฉากกับแกนมองของกล้อง (ตัดความต่างเชิงลึกทิ้ง)
    float LateralDistanceTo(Vector3 target)
    {
        Vector3 f = activeCamera != null ? activeCamera.transform.forward : Vector3.forward;
        Vector3 d = target - transform.position;
        return (d - f * Vector3.Dot(d, f)).magnitude;
    }

    // จุดที่ตรงกับเป้าในระนาบกล้อง แต่คงระยะลึกปัจจุบันของถ้วยไว้
    // (กล้องหน้าตรง: เทียบเท่า (target.x, target.y, cup.z) แบบโค้ดเดิมเป๊ะ)
    Vector3 AlignLaterallyWith(Vector3 target)
    {
        Vector3 f = activeCamera != null ? activeCamera.transform.forward : Vector3.forward;
        Vector3 d = target - transform.position;
        return transform.position + d - f * Vector3.Dot(d, f);
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