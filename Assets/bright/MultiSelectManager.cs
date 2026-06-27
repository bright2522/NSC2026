using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ตัวจัดการกลาง: สร้างแถวจากข้อมูล + เก็บว่าตอนนี้เลือกอะไรไว้บ้าง
public class MultiSelectManager : MonoBehaviour
{
    [Header("Prefab & Container")]
    public SelectableItem itemPrefab;   // ลาก prefab ของแถวมาใส่
    public Transform contentParent;     // ลาก Content ของ Scroll View มาใส่

    [Header("UI เสริม (จะใส่หรือไม่ก็ได้)")]
    public TMP_Text countLabel;         // ป้ายบอกว่าเลือกไปกี่อัน
    public Button selectAllButton;      // ปุ่มเลือกทั้งหมด
    public Button clearButton;          // ปุ่มล้างการเลือก
    public Button confirmButton;        // ปุ่มยืนยัน

    // เก็บแถวทั้งหมด และเก็บเฉพาะอันที่ถูกเลือก
    private readonly List<SelectableItem> items = new List<SelectableItem>();
    private readonly HashSet<SelectableItem> selected = new HashSet<SelectableItem>();

    // event ให้สคริปต์อื่นมา subscribe เวลามีการเลือกเปลี่ยน
    public System.Action<List<SelectableItem>> OnSelectionChanged;

    void Start()
    {
        if (selectAllButton) selectAllButton.onClick.AddListener(SelectAll);
        if (clearButton)     clearButton.onClick.AddListener(ClearAll);
        if (confirmButton)   confirmButton.onClick.AddListener(Confirm);

        UpdateUI();
    }

    // สร้างแถวจากข้อมูล (id, ชื่อ)
    public void Populate(IEnumerable<(string id, string name)> data)
    {
        Clear();
        foreach (var d in data)
        {
            var item = Instantiate(itemPrefab, contentParent);
            item.Init(this, d.id, d.name);
            items.Add(item);
        }
        UpdateUI();
    }

    // ลบแถวทั้งหมดทิ้ง
    public void Clear()
    {
        foreach (var item in items)
            if (item) Destroy(item.gameObject);

        items.Clear();
        selected.Clear();
        UpdateUI();
    }

    // ถูกเรียกจาก SelectableItem ทุกครั้งที่ติ๊ก/ยกเลิกติ๊ก
    public void OnItemToggled(SelectableItem item, bool isOn)
    {
        if (isOn) selected.Add(item);
        else      selected.Remove(item);

        UpdateUI();
        OnSelectionChanged?.Invoke(GetSelected());
    }

    public void SelectAll()
    {
        foreach (var item in items) item.SetSelected(true);
    }

    public void ClearAll()
    {
        foreach (var item in items) item.SetSelected(false);
    }

    // ดึงรายการที่เลือกออกมาใช้งาน
    public List<SelectableItem> GetSelected() => selected.ToList();

    // ดึงเฉพาะ id ที่เลือก (เอาไปบันทึก/ส่งต่อได้ง่าย ๆ)
    public List<string> GetSelectedIds() => selected.Select(s => s.itemId).ToList();

    private void UpdateUI()
    {
        if (countLabel)
            countLabel.text = $"เลือกแล้ว {selected.Count}/{items.Count}";
    }

    private void Confirm()
    {
        var names = string.Join(", ", selected.Select(s => s.itemName));
        Debug.Log($"ยืนยันการเลือก ({selected.Count}): {names}");
        // TODO: เอา GetSelectedIds() ไปทำอะไรต่อตรงนี้ได้เลย
    }
}