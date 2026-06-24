using UnityEngine;

public class GarlicCupController : MonoBehaviour
{
    private bool isDragging = false;
    private bool isAbovePan = false;
    private bool isGarlicReleased = false; 
    private bool isSnapping = false; 
    private bool isReturningToStart = false; 
    
    private Camera mainCamera;
    private Vector3 offset;
    private float zCoord;
    private Vector3 targetSnapPosition; 

    private Vector3 initialPosition;    
    private Quaternion initialRotation; 

    [Header("Settings")]
    public float tiltSpeed = 150.0f;     
    public float snapSpeed = 8.0f;     

    [Header("ระบบกำหนดองศาปล่อยกระเทียม")]
    public float minReleaseAngle = 25.0f; 

    [Header("ระบบล็อกอิสระ & แรงดูด")]
    public Transform lockTarget;       
    public float snapDistance = 3.0f;  

    [Header("ระบบเสกกระเทียม (Spawning)")]
    public GameObject garlicPrefab;    
    public Transform spawnPoint;       
    public int garlicAmount = 100;       
    public float spawnInterval = 0.03f; 

    private float currentRotationZ = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
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
                isGarlicReleased = false; 
            }
        }

        if (isAbovePan && !isDragging && !isSnapping && !isReturningToStart)
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
                isReturningToStart = false; 
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
        Vector3 acceleration = Input.acceleration;
        if (acceleration != Vector3.zero) inputX = acceleration.x;

        if (inputX == 0)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputX = -1f;
        }

        if (inputX < 0)
        {
            currentRotationZ -= inputX * tiltSpeed * Time.deltaTime;
        }
        else
        {
            currentRotationZ = Mathf.MoveTowards(currentRotationZ, 0f, tiltSpeed * 0.75f * Time.deltaTime);
        }

        currentRotationZ = Mathf.Clamp(currentRotationZ, 0f, 60f);
        transform.localRotation = Quaternion.Euler(0f, 0f, currentRotationZ);

        if (currentRotationZ > minReleaseAngle && !isGarlicReleased)
        {
            isGarlicReleased = true;
            StartCoroutine(SpawnGarlicRoutine());
        }
    }

    System.Collections.IEnumerator SpawnGarlicRoutine()
    {
        for (int i = 0; i < garlicAmount; i++)
        {
            if (garlicPrefab != null && spawnPoint != null)
            {
                float randomX = Random.Range(-0.06f, 0.06f);
                float randomY = Random.Range(-0.03f, 0.03f);
                float randomZ = Random.Range(-0.06f, 0.06f);
                Vector3 spawnOffset = (spawnPoint.right * randomX) + (spawnPoint.up * randomY) + (spawnPoint.forward * randomZ);

                GameObject spawnedGarlic = Instantiate(garlicPrefab, spawnPoint.position + spawnOffset, Random.rotation);
                
                Rigidbody rb = spawnedGarlic.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }

        isAbovePan = false;
        isReturningToStart = true; 
    }
}