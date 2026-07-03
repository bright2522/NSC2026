using UnityEngine;

public class BottleDrag : MonoBehaviour
{
    [Header("References")]
    public Transform bottleSnapPoint; // (ลากจุดล็อกที่กระทะมาใส่)
    public BottleTilt bottleTiltScript; // (ลากสคริปต์ BottleTilt มาใส่)
    
    [Header("Settings")]
    public float snapDistance = 1.0f; // ระยะห่างที่จะเริ่มดูดเข้าจุด
    public float snapSpeed = 10f;

    private bool isSnapped = false;
    private Vector3 screenPoint;
    private Vector3 offset;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        
        // ถ้าลืมลากมาใส่ใน Inspector โค้ดจะพยายามหาเองให้อัตโนมัติ
        if (bottleTiltScript == null)
        {
            bottleTiltScript = GetComponent<BottleTilt>();
        }
    }

    void Update()
    {
        // ถ้ายกขวดขึ้นมาแล้วและเคลื่อนไปใกล้จุดกระทะ ให้ล็อกขวดเข้าจุด
        if (bottleSnapPoint != null && !isSnapped)
        {
            float distance = Vector3.Distance(transform.position, bottleSnapPoint.position);
            
            if (distance < snapDistance)
            {
                SnapToPan();
            }
        }
    }

    // --- 🛠️ ส่วนโค้ดสำหรับคลิกลากด้วยเมาส์ / นิ้วบนมือถือ ---
    
    void OnMouseDown()
    {
        // ถ้าระบบล็อกไปแล้ว จะไม่ให้ลากขวดหนีไปไหนได้อีก (หรือตามกติกาเกมคุณ)
        if (isSnapped) return;

        screenPoint = mainCamera.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    void OnMouseDrag()
    {
        if (isSnapped) return;

        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = mainCamera.ScreenToWorldPoint(curScreenPoint) + offset;
        
        // อัปเดตตำแหน่งขวดตามตำแหน่งเมาส์/นิ้ว
        transform.position = curPosition;
    }

    // --------------------------------------------------

    void SnapToPan()
    {
        isSnapped = true;

        // ดูดตำแหน่งขวดเข้าหาจุด Snap ให้เป๊ะๆ
        transform.position = bottleSnapPoint.position;
        
        // สั่งเปิดระบบเอียงขวดในสคริปต์ BottleTilt
        if (bottleTiltScript != null)
        {
            bottleTiltScript.canTilt = true; 
        }
    }

    // หากต้องการให้ยกขวดออกจากกระทะได้อีกครั้ง ให้เรียกใช้ฟังก์ชันนี้
    public void ResetBottle()
    {
        isSnapped = false;
        if (bottleTiltScript != null)
        {
            bottleTiltScript.canTilt = false;
        }
    }
}