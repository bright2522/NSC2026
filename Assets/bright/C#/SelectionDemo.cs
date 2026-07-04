using System.Collections.Generic;
using UnityEngine;

// โหมดวางการ์ดเองในซีน — ตัวนี้แค่สั่ง Manager ให้สุ่มของหมดตอนเปิดตู้ครั้งแรก
public class SelectionDemo : MonoBehaviour
{
    public MultiSelectManager manager;

    private bool built = false;

    // ผูกกับ event "On First Open" ของตู้เย็น
    public void BuildIngredients()
    {
        if (built) return;   // สุ่มครั้งเดียว
        built = true;

        manager.SetupSceneItems();             // สุ่มของหมดให้การ์ดในซีน
        manager.OnSelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(List<SelectableItem> selectedItems)
    {
        Debug.Log($"เลือกอยู่ {selectedItems.Count} อัน");
    }
}