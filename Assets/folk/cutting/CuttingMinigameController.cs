using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ติดตามความคืบหน้าการหั่น ไม่ต้องเป็น step ใน MinigameFlowManager
/// แค่โยน event OnAllFoodSliced → ปุ่ม Next เปิดเอง
/// </summary>
public class CuttingMinigameController : MonoBehaviour
{
    [Header("อาหารที่ต้องหั่น (ลาก SliceableFood prefab/GameObject มาใส่)")]
    [SerializeField] private List<SliceableFood> requiredFoods = new List<SliceableFood>();

    [Header("ถ้า true = นับจากจำนวนชิ้นที่เกิดขึ้น ไม่ใช่ destroy ต้นฉบับ")]
    [SerializeField] private bool countByPieceCount = false;
    [SerializeField] private int requiredPieceCount = 4;

    [Header("UI / ปุ่มที่จะเปิดหลังหั่นครบ")]
    [SerializeField] private GameObject nextButtonObject;

    public event Action OnAllFoodSliced;
    public UnityEvent onAllFoodSlicedUnity;

    public int TotalRequired => requiredFoods.Count;
    public int SlicedCount { get; private set; }
    public bool IsCompleted { get; private set; }

    private int trackedCount;
    private int currentPieceCount;

    private void OnEnable()
    {
        SlicedCount = 0;
        IsCompleted = false;
        currentPieceCount = 0;

        if (nextButtonObject != null)
            nextButtonObject.SetActive(false);

        if (countByPieceCount)
        {
            // ติดตามผ่าน SlicedFoodManager — poll ทุก frame
            trackedCount = CountCurrentPieces();
        }
        else
        {
            trackedCount = requiredFoods.Count;
            RegisterDestroyWatchers();
        }
    }

    private void OnDisable()
    {
        UnregisterDestroyWatchers();
    }

    private void Update()
    {
        if (IsCompleted) return;

        if (countByPieceCount)
        {
            int pieces = CountCurrentPieces();
            if (pieces > currentPieceCount)
            {
                currentPieceCount = pieces;
                SlicedCount = currentPieceCount;
                CheckCompletion();
            }
        }
    }

    // เรียกจาก SliceableFoodWatcher เมื่อ SliceableFood ต้นฉบับถูก Destroy
    internal void NotifyFoodSliced(SliceableFood food)
    {
        if (IsCompleted) return;
        SlicedCount++;
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if (IsCompleted) return;

        bool done = countByPieceCount
            ? currentPieceCount >= requiredPieceCount
            : SlicedCount >= trackedCount;

        if (!done) return;

        IsCompleted = true;

        if (nextButtonObject != null)
            nextButtonObject.SetActive(true);

        OnAllFoodSliced?.Invoke();
        onAllFoodSlicedUnity?.Invoke();

        Debug.Log("[CuttingMinigame] หั่นครบแล้ว!");
    }

    private void RegisterDestroyWatchers()
    {
        foreach (var food in requiredFoods)
        {
            if (food == null) continue;
            var watcher = food.GetComponent<SliceableFoodWatcher>();
            if (watcher == null) watcher = food.gameObject.AddComponent<SliceableFoodWatcher>();
            watcher.Initialize(food, this);
        }
    }

    private void UnregisterDestroyWatchers()
    {
        foreach (var food in requiredFoods)
        {
            if (food == null) continue;
            var watcher = food.GetComponent<SliceableFoodWatcher>();
            if (watcher != null) watcher.Unlink();
        }
    }

    private static int CountCurrentPieces()
    {
        if (SlicedFoodManager.Instance == null) return 0;
        return SlicedFoodManager.Instance.transform.childCount;
    }
}

/// <summary>
/// ตัวช่วยติดกับ SliceableFood แต่ละชิ้น เพื่อรับรู้ตอนมัน Destroy (ถูกหั่น)
/// </summary>
internal sealed class SliceableFoodWatcher : MonoBehaviour
{
    private SliceableFood target;
    private CuttingMinigameController controller;
    private bool linked;

    public void Initialize(SliceableFood food, CuttingMinigameController ctrl)
    {
        target = food;
        controller = ctrl;
        linked = true;
    }

    public void Unlink()
    {
        linked = false;
    }

    private void OnDestroy()
    {
        if (linked && controller != null && target != null)
        {
            controller.NotifyFoodSliced(target);
        }
    }
}
