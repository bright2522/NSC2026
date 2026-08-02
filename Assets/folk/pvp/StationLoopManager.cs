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

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.UpdateScoreUI();
        }

        Debug.Log("<color=cyan><b>สร้างสเตชันชุดใหม่สำเร็จ! พร้อมเล่นต่อ</b></color>");
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