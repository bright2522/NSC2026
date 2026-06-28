using UnityEngine;

public class SausageItemController : MonoBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = GetComponentInChildren<Rigidbody>();
    }

    public void SetPhysicsLock(bool shouldLock)
    {
        if (rb != null)
        {
            // ถ้าสั่งปลดล็อก (shouldLock = false) ให้ฟิสิกส์กลับมาทำงานปกติ
            rb.isKinematic = shouldLock;
            rb.useGravity = !shouldLock;
        }

        if (!shouldLock)
        {
            // 🔥 บล็อกไม้ตาย: เมื่อเลิกล็อกแล้ว ให้ตัดขาดจากวัตถุแม่ทันทีชัวร์ๆ 
            transform.SetParent(null);
        }
    }
}