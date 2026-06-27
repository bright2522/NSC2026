using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MultiSelectManager : MonoBehaviour
{
    [Header("Prefab & Container")]
    public SelectableItem itemPrefab;
    public Transform contentParent;

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

    public void Populate(IEnumerable<IngredientData> data)
    {
        Clear();
        foreach (var d in data)
        {
            var item = Instantiate(itemPrefab, contentParent);
            item.Init(this, d);
            items.Add(item);
        }
        UpdateUI();
    }

    public void Clear()
    {
        foreach (var item in items) if (item) Destroy(item.gameObject);
        items.Clear();
        selected.Clear();
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