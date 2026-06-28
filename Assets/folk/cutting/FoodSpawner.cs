using UnityEngine;

// วาง/สร้าง prefab อาหารที่ตัดได้เข้าฉากตอนรัน
public class FoodSpawner : MonoBehaviour
{
    [Header("Prefab & ตำแหน่ง")]
    public GameObject foodPrefab;
    public Transform spawnPoint;

    private GameObject current;

    public void SpawnFood()
    {
        if (foodPrefab == null)
        {
            Debug.LogWarning("ยังไม่ได้ใส่ foodPrefab");
            return;
        }

        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint ? spawnPoint.rotation : transform.rotation;

        current = Instantiate(foodPrefab, pos, rot);
    }

    // วางชิ้นใหม่ (ลบของเก่าทิ้งก่อน) — เหมาะกับปุ่ม "เริ่มใหม่"
    public void RespawnFood()
    {
        if (current != null) Destroy(current);
        SpawnFood();
    }
}