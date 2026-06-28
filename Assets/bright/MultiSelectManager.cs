using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MultiSelectManager : MonoBehaviour
{
    [Header("โหมดวางการ์ดเองในซีน")]
    [Tooltip("ลากการ์ดทุกใบที่วางไว้ในซีนมาใส่ที่นี่")]
    public List<SelectableItem> sceneItems = new List<SelectableItem>();
    [Tooltip("โอกาสที่แต่ละใบจะสุ่มเป็นของหมด (0-1)")]
    [Range(0f, 1f)] public float outOfStockChance = 0.3f;

    [Header("UI เสริม (จะใส่หรือไม่ก็ได้)")]
    public TMP_Text countLabel;
    public TMP_Text buyTotalLabel;
    public Button selectAllButton;
    public Button clearButton;
    public Button confirmButton;

    private readonly List<SelectableItem> items = new List<SelectableItem>();
    private readonly HashSet<SelectableItem> selected = new HashSet<SelectableItem>();

    public System.Action<List<SelectableItem>> OnSelectionChanged;

    void Start()
    {
        if (selectAllButton) selectAllButton.onClick.AddListener(SelectAll);
        if (clearButton)     clearButton.onClick.AddListener(ClearAll);
        if (confirmButton)   confirmButton.onClick.AddListener(Confirm);
        UpdateUI();
    }

    // เรียกตอนเปิดตู้เย็นครั้งแรก — สุ่มของหมดให้การ์ดที่วางไว้เองในซีน
    public void SetupSceneItems()
    {
        items.Clear();
        selected.Clear();

        foreach (var item in sceneItems)
        {
            if (item == null) continue;

            // สุ่มว่าใบนี้หมดไหม
            bool outOfStock = Random.value < outOfStockChance;
            item.SetupInScene(this, outOfStock);
            items.Add(item);
        }
        UpdateUI();
    }

    public void OnItemToggled(SelectableItem item, bool isOn)
    {
        if (isOn) selected.Add(item);
        else      selected.Remove(item);
        UpdateUI();
        OnSelectionChanged?.Invoke(GetSelected());
    }

    public void SelectAll() { foreach (var item in items) item.SetSelected(true); }
    public void ClearAll()  { foreach (var item in items) item.SetSelected(false); }

    public List<SelectableItem> GetSelected() => selected.ToList();
    public List<string> GetSelectedIds() => selected.Select(s => s.itemId).ToList();

    public int GetOutOfStockTotal()
        => items.Where(i => i.isOutOfStock).Sum(i => i.price);

    void UpdateUI()
    {
        if (countLabel)    countLabel.text    = $"เลือกแล้ว {selected.Count}/{items.Count}";
        if (buyTotalLabel) buyTotalLabel.text = $"ต้องซื้อเพิ่ม {GetOutOfStockTotal()} บาท";
    }

    void Confirm()
    {
        var names = string.Join(", ", selected.Select(s => s.itemName));
        Debug.Log($"ยืนยัน ({selected.Count}): {names} | ต้องซื้อเพิ่ม {GetOutOfStockTotal()} บาท");
    }
}