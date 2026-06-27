using UnityEngine;

// วาง/สร้าง prefab อาหารที่ตัดได้เข้าฉากตอนรัน
public class FoodSpawner : MonoBehaviour
{
    [Header("Prefab & ตำแหน่ง")]
    public GameObject foodPrefab;      // prefab อาหารที่จะตัด
    public Transform spawnPoint;       // จุดวาง (ถ้าไม่ใส่ จะใช้ตำแหน่งของ spawner เอง)

    [Header("Material เนื้อในตอนตัด")]
    public Material insideMaterial;    // เผื่อ prefab ยังไม่ได้เซ็ต จะใส่ให้อัตโนมัติ

    private GameObject current;        // ชิ้นที่วางอยู่ตอนนี้

    // เรียกจากปุ่ม หรือจากสคริปต์อื่นก็ได้
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

        // ถ้า prefab ลืมเซ็ต insideMaterial ก็ยัดให้ตรงนี้
        var slice = current.GetComponent<SliceableFood>();
        if (slice != null && slice.insideMaterial == null)
            slice.insideMaterial = insideMaterial;
    }

    // วางชิ้นใหม่ (ลบของเก่าทิ้งก่อน) — เหมาะกับปุ่ม "เริ่มใหม่"
    public void RespawnFood()
    {
        if (current != null) Destroy(current);
        SpawnFood();
    }
}