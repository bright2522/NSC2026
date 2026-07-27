using UnityEngine;

public class DetachCamera : MonoBehaviour
{
    void Awake()
    {
        // ปลดกล้องออกจาก Parent เพื่อไม่ให้เลื่อนตามการปัดสเตชัน
        transform.SetParent(null);
    }
}