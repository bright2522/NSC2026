using UnityEngine;

public class StationLoopManager : MonoBehaviour
{
    public static StationLoopManager Instance { get; private set; }

    [Header("Station Setup")]
    [Tooltip("ลาก Prefab ชุดสเตชันทั้งหมดมาใส่ตรงนี้")]
    public GameObject stationSetPrefab; 

    [Tooltip("ระยะห่างที่จะวางสเตชันใหม่ (เพื่อไม่ให้ซ้อนทับกับของเก่า)")]
    public Vector3 spawnOffset = new Vector3(50f, 0f, 0f);

    private GameObject currentStationInstance;
    private Camera currentActiveCamera;
    private Vector3 lastSpawnPosition = Vector3.zero;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (currentStationInstance == null && stationSetPrefab != null)
        {
            SpawnNextStation();
        }
    }

    public void SpawnNextStation()
    {
        if (stationSetPrefab == null)
        {
            Debug.LogError("[StationLoopManager] ไม่ไม่ได้ใส่ stationSetPrefab ใน Inspector!");
            return;
        }

        // 1. คำนวณตำแหน่งใหม่เพื่อขยับไปด้านข้าง
        Vector3 newSpawnPos = lastSpawnPosition + spawnOffset;

        // 2. เคลียร์และลบกล้องตัวเก่าทิ้งทั้งหมด (แม้ว่าจะถอดออกจาก Parent ไปแล้วก็ตาม)
        CleanupOldCameras();

        // 3. ลบ Prefab สเตชันเก่าออก
        if (currentStationInstance != null)
        {
            Destroy(currentStationInstance, 0.1f);
        }

        // 4. โคลนชุดสเตชันใหม่ขึ้นมา
        GameObject newStation = Instantiate(stationSetPrefab, newSpawnPos, Quaternion.identity);

        // 5. ค้นหากล้องในสเตชันใหม่ และตั้งค่าให้เป็น MainCamera ตัวหลัก
        Camera newCam = newStation.GetComponentInChildren<Camera>(true);
        if (newCam != null)
        {
            newCam.enabled = true;
            newCam.tag = "MainCamera";
            currentActiveCamera = newCam;
        }

        // 6. อัปเดต Reference สเตชันปัจจุบัน
        currentStationInstance = newStation;
        lastSpawnPosition = newSpawnPos;

        // 7. อัปเดต UI คะแนน
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.UpdateScoreUI();
        }

        Debug.Log("<color=cyan><b>สร้างสเตชันชุดใหม่สำเร็จ! พร้อมเล่นต่อ</b></color>");
    }

    // 💡 ฟังก์ชันพิเศษ: ตามลบและปิดกล้องเก่าทุกตัวใน Scene ไม่ว่าจะโดน Unparent ไปไว้ไหนก็ตาม
    private void CleanupOldCameras()
    {
        // 1. ลบกล้องที่อ้างอิงไว้ในรอบก่อนหน้า (ถ้าโดน Unparent ออกไป)
        if (currentActiveCamera != null)
        {
            Destroy(currentActiveCamera.gameObject);
            currentActiveCamera = null;
        }

        // 2. กวาดล้างกล้องที่ติด Tag MainCamera ทั้งหมดใน Scene อีกรอบเพื่อความชัวร์
        Camera[] allCameras = Camera.allCameras;
        foreach (Camera cam in allCameras)
        {
            if (cam != null && cam.CompareTag("MainCamera"))
            {
                // เปลี่ยน Tag ปิดการทำงาน แล้วสั่งทำลายทิ้งทันที
                cam.tag = "Untagged";
                cam.enabled = false;
                Destroy(cam.gameObject);
            }
        }
    }
}