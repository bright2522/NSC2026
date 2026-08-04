using UnityEngine;

public class StationLoopManager : MonoBehaviour
{
    public static StationLoopManager Instance { get; private set; }

    [Header("Station Setup")]
    [Tooltip("ลาก Prefab ชุดสเตชันทั้งหมดมาใส่ตรงนี้")]
    public GameObject stationSetPrefab; 

    [Tooltip("ระยะห่างที่จะวางสเตชันใหม่")]
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
            Debug.LogError("[StationLoopManager] ยังไม่ได้ใส่ stationSetPrefab ใน Inspector!");
            return;
        }

        Vector3 newSpawnPos = lastSpawnPosition + spawnOffset;

        CleanupOldCameras();

        if (currentStationInstance != null)
        {
            Destroy(currentStationInstance, 0.1f);
        }

        GameObject newStation = Instantiate(stationSetPrefab, newSpawnPos, Quaternion.identity);

        Camera newCam = newStation.GetComponentInChildren<Camera>(true);
        if (newCam != null)
        {
            newCam.enabled = true;
            newCam.tag = "MainCamera";
            currentActiveCamera = newCam;
        }

        currentStationInstance = newStation;
        lastSpawnPosition = newSpawnPos;

        // กันมี EventSystem ซ้อนกันหลายตัวในซีน (ทำให้ปุ่ม UI กดไม่ติด)
        CleanupExtraEventSystems(newStation);

        // รีเซ็ตสถานะ "ใส่วัตถุดิบครบแล้ว" ของสเตชันเก่า ไม่งั้นสเตชันใหม่จะหยิบตะหลิวได้ทันทีทั้งที่ยังไม่ได้ใส่อะไร
        if (PanPrepManager.Instance != null)
        {
            PanPrepManager.Instance.ResetPrep();
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.UpdateScoreUI();
        }

        Debug.Log("<color=cyan><b>สร้างสเตชันชุดใหม่สำเร็จ! พร้อมเล่นต่อ</b></color>");
    }

    // 💡 ป้องกัน EventSystem ซ้ำซ้อน: prefab สเตชันแต่ละอันมี EventSystem ติดมาด้วย
    // ทำให้ทุกครั้งที่ Spawn สเตชันใหม่ จะมี EventSystem เพิ่มขึ้นเรื่อยๆ จนปุ่ม UI กดไม่ติด
    private void CleanupExtraEventSystems(GameObject keepInstance)
    {
        UnityEngine.EventSystems.EventSystem[] allSystems =
            FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);

        UnityEngine.EventSystems.EventSystem keep = keepInstance != null
            ? keepInstance.GetComponentInChildren<UnityEngine.EventSystems.EventSystem>(true)
            : null;

        if (keep == null && allSystems.Length > 0)
        {
            keep = allSystems[allSystems.Length - 1];
        }

        for (int i = 0; i < allSystems.Length; i++)
        {
            if (allSystems[i] != null && allSystems[i] != keep)
            {
                Destroy(allSystems[i].gameObject);
            }
        }
    }

    private void CleanupOldCameras()
    {
        if (currentActiveCamera != null)
        {
            Destroy(currentActiveCamera.gameObject);
            currentActiveCamera = null;
        }

        Camera[] allCameras = Camera.allCameras;
        foreach (Camera cam in allCameras)
        {
            if (cam != null && cam.CompareTag("MainCamera"))
            {
                cam.tag = "Untagged";
                cam.enabled = false;
                Destroy(cam.gameObject);
            }
        }
    }
}