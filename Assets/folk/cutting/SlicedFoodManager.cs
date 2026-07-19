using System.Collections.Generic;
using UnityEngine;

public class SlicedFoodManager : MonoBehaviour
{
    public static SlicedFoodManager Instance { get; private set; }

    [SerializeField] private string containerName = "SlicedPieces";

    // ชิ้นที่หั่นแล้วอยู่ใต้ parent ของสเตชันตัวเอง (ไม่ใช่ลูกของ manager นี้) เพื่อให้เลื่อนตาม
    // SwipeStationSlider ได้ จึงต้องนับจำนวนผ่าน registry แทน transform.childCount
    private readonly HashSet<GameObject> trackedPieces = new HashSet<GameObject>();

    public int PieceCount => trackedPieces.Count;

    public static Transform Container
    {
        get
        {
            if (Instance != null)
                return Instance.transform;

            SlicedFoodManager existing = FindObjectOfType<SlicedFoodManager>();
            if (existing != null)
                return existing.transform;

            GameObject managerObject = new GameObject("SlicedFoodManager");
            return managerObject.AddComponent<SlicedFoodManager>().transform;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameObject.name = containerName;
    }

    public static void RegisterPiece(GameObject piece)
    {
        if (piece == null) return;
        if (Instance == null) _ = Container; // สร้าง instance ถ้ายังไม่มี
        Instance.trackedPieces.Add(piece);
    }

    public static void UnregisterPiece(GameObject piece)
    {
        Instance?.trackedPieces.Remove(piece);
    }

    public void ClearAllSlicedPieces()
    {
        foreach (GameObject piece in trackedPieces)
            if (piece != null) Destroy(piece);
        trackedPieces.Clear();
    }
}
