using UnityEngine;
using UnityEngine.UI;
using TMPro; // ถ้าไม่ใช้ TextMeshPro ให้เปลี่ยนเป็น UnityEngine.UI.Text

// สคริปต์นี้ติดไว้กับ "แต่ละแถว" ที่ติ๊กเลือกได้
[RequireComponent(typeof(Toggle))]
public class SelectableItem : MonoBehaviour
{
    public string itemId;     // ไอดีของไอเทม
    public string itemName;   // ชื่อที่แสดงบนหน้าจอ

    [Header("Visual Feedback (จะใส่หรือไม่ก็ได้)")]
    public GameObject selectedFrame;   // กรอบสีเขียวที่จะโผล่ตอนเลือก
    public Image background;            // พื้นหลังแถว (ไว้เปลี่ยนสี)
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.85f, 1f, 0.85f); // เขียวอ่อน

    private Toggle toggle;
    private MultiSelectManager manager;

    public bool IsSelected => toggle != null && toggle.isOn;

    // เรียกจาก Manager ตอนสร้างแถว
    public void Init(MultiSelectManager mgr, string id, string name)
    {
        manager = mgr;
        itemId = id;
        itemName = name;

        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggleChanged);

        var label = GetComponentInChildren<TMP_Text>();
        if (label != null) label.text = name;

        UpdateVisual(toggle.isOn); // ตั้งหน้าตาเริ่มต้น
    }

    private void OnToggleChanged(bool isOn)
    {
        UpdateVisual(isOn);
        if (manager != null) manager.OnItemToggled(this, isOn);
    }

    // อัปเดตหน้าตา: โชว์/ซ่อนกรอบเขียว + เปลี่ยนสีพื้นหลัง
    void UpdateVisual(bool isOn)
    {
        if (selectedFrame != null) selectedFrame.SetActive(isOn);
        if (background != null) background.color = isOn ? selectedColor : normalColor;
    }

    // สั่งติ๊ก/ยกเลิกจากโค้ด
    public void SetSelected(bool value, bool notify = true)
    {
        if (toggle == null) toggle = GetComponent<Toggle>();

        if (notify)
        {
            toggle.isOn = value; // จะ fire event แล้วไปอัปเดตหน้าตาเอง
        }
        else
        {
            toggle.SetIsOnWithoutNotify(value);
            UpdateVisual(value); // ต้องอัปเดตหน้าตาเอง เพราะไม่ได้ fire event
        }
    }
}