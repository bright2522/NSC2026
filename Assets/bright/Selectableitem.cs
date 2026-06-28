using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Toggle))]
public class SelectableItem : MonoBehaviour
{
    [Header("ข้อมูลวัตถุดิบ (ตั้งเองที่การ์ดแต่ละใบ)")]
    public string itemId;
    public string itemName;
    public int price;          // ราคาเวลาต้องซื้อเพิ่ม (บาท)

    [HideInInspector] public bool isOutOfStock; // เซ็ตตอนรัน (สุ่ม)

    [Header("ป้ายข้อความ (ลากใส่เท่าที่มี)")]
    public TMP_Text nameLabel;
    public TMP_Text priceLabel;
    public TMP_Text statusLabel;

    [Header("Visual Feedback")]
    public GameObject selectedFrame;
    public GameObject outOfStockOverlay;
    public Image background;
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.85f, 1f, 0.85f);
    public Color outOfStockColor = new Color(0.8f, 0.8f, 0.8f);

    private Toggle toggle;
    private MultiSelectManager manager;

    public bool IsSelected => toggle != null && toggle.isOn;

    // เรียกจาก Manager ตอนเปิดตู้เย็น (โหมดวางการ์ดเองในซีน)
    public void SetupInScene(MultiSelectManager mgr, bool outOfStock)
    {
        manager = mgr;
        isOutOfStock = outOfStock;

        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.RemoveListener(OnToggleChanged); // กันผูกซ้ำ
        toggle.onValueChanged.AddListener(OnToggleChanged);

        if (nameLabel) nameLabel.text = itemName;

        toggle.isOn = false;                 // เริ่มมาแบบยังไม่เลือก
        toggle.interactable = !isOutOfStock; // ของหมด กดไม่ได้
        if (outOfStockOverlay) outOfStockOverlay.SetActive(isOutOfStock);
        if (statusLabel) statusLabel.text = isOutOfStock ? "หมด" : "";
        if (priceLabel)  priceLabel.text  = isOutOfStock ? $"{price} บาท" : "";

        UpdateVisual(false);
    }

    private void OnToggleChanged(bool isOn)
    {
        UpdateVisual(isOn);
        if (manager != null) manager.OnItemToggled(this, isOn);
    }

    void UpdateVisual(bool isOn)
    {
        if (selectedFrame != null) selectedFrame.SetActive(isOn && !isOutOfStock);

        if (background != null)
        {
            if (isOutOfStock) background.color = outOfStockColor;
            else background.color = isOn ? selectedColor : normalColor;
        }
    }

    public void SetSelected(bool value, bool notify = true)
    {
        if (toggle == null) toggle = GetComponent<Toggle>();
        if (isOutOfStock) return;

        if (notify) toggle.isOn = value;
        else { toggle.SetIsOnWithoutNotify(value); UpdateVisual(value); }
    }
}