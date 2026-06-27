using System.Collections.Generic;
using UnityEngine;

// สคริปต์ตัวอย่าง: ป้อนข้อมูลให้ Manager สร้างรายการให้ดู
public class SelectionDemo : MonoBehaviour
{
    public MultiSelectManager manager;

    void Start()
    {
        // ตัวอย่างข้อมูล — เปลี่ยนเป็นข้อมูลจริงของโปรเจกต์ได้เลย
        var data = new List<(string id, string name)>
        {
            ("01", "มะม่วง"),
            ("02", "กล้วย"),
            ("03", "ส้ม"),
            ("04", "แอปเปิล"),
            ("05", "องุ่น"),
        };

        manager.Populate(data);

        // ฟังเวลามีการเลือกเปลี่ยน (จะใส่หรือไม่ก็ได้)
        manager.OnSelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(List<SelectableItem> selectedItems)
    {
        Debug.Log($"ตอนนี้เลือกอยู่ {selectedItems.Count} อัน");
    }
}