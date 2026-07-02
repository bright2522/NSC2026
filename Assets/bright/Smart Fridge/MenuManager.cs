using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

[System.Serializable]
public class MenuData
{
    public string menuName;
    public List<string> requiredIds = new List<string>();
}

[System.Serializable]
public class StoreData
{
    public string storeName;
    public float distanceMeters;
    [Tooltip("id วัตถุดิบที่ร้านนี้มีขาย (เว้นว่าง = มีทุกอย่าง)")]
    public List<string> sellsIds = new List<string>();
}

public class MenuManager : MonoBehaviour
{
    [Header("เชื่อมกับระบบเลือกวัตถุดิบ")]
    public MultiSelectManager selectionManager;

    [Header("รายการเมนู")]
    public List<MenuData> menus = new List<MenuData>();

    [Header("รายการร้านค้า")]
    public List<StoreData> stores = new List<StoreData>();

    [Header("ป้ายข้อความผลลัพธ์")]
    public TMP_Text missingText;
    public List<TMP_Text> storeTexts = new List<TMP_Text>();

    private MenuData currentMenu;
    private int currentIndex = -1;

    // ร้านเรียงตามระยะ (ตรงกับที่โชว์บนจอ) — ปุ่มร้านอ้างอิงลิสต์นี้
    private List<StoreData> displayedStores = new List<StoreData>();

    public void OnMenuClicked(int menuIndex)
    {
        if (menuIndex < 0 || menuIndex >= menus.Count) return;

        if (menuIndex == currentIndex)
        {
            ClearDisplay();
            return;
        }

        currentIndex = menuIndex;
        currentMenu = menus[menuIndex];
        Refresh();
    }

    void ClearDisplay()
    {
        currentIndex = -1;
        currentMenu = null;
        if (missingText) missingText.text = "";
        foreach (var t in storeTexts)
            if (t != null) t.gameObject.SetActive(false);
    }

    void Refresh()
    {
        if (currentMenu == null) return;

        var missing = new List<SelectableItem>();
        foreach (var id in currentMenu.requiredIds)
        {
            var item = selectionManager.GetItemById(id);
            if (item != null && item.isOutOfStock) missing.Add(item);
        }

        if (missingText)
        {
            missingText.text = missing.Count == 0
                ? "วัตถุดิบครบแล้ว!"
                : "ขาด: " + string.Join(", ", missing.Select(m => m.itemName));
        }

        ShowStores();
    }

    void ShowStores()
    {
        displayedStores = stores.OrderBy(s => s.distanceMeters).ToList();

        for (int i = 0; i < storeTexts.Count; i++)
        {
            if (storeTexts[i] == null) continue;

            if (i < displayedStores.Count)
            {
                var s = displayedStores[i];
                storeTexts[i].text = $"{s.storeName}  {FormatDistance(s.distanceMeters)}";
                storeTexts[i].gameObject.SetActive(true);
            }
            else
            {
                storeTexts[i].gameObject.SetActive(false);
            }
        }
    }

    string FormatDistance(float meters)
    {
        if (meters >= 1000f)
            return $"{(meters / 1000f):0.#} km";
        return $"{meters:0} m";
    }

    // *** ปุ่มร้าน: ซื้อของที่ขาดจากร้านช่องนี้ (0=บนสุด, 1=กลาง, 2=ล่าง) ***
    public void BuyFromStore(int slotIndex)
    {
        if (currentMenu == null) return;
        if (slotIndex < 0 || slotIndex >= displayedStores.Count) return;

        StoreData store = displayedStores[slotIndex];

        foreach (var id in currentMenu.requiredIds)
        {
            var item = selectionManager.GetItemById(id);
            if (item == null || !item.isOutOfStock) continue;

            // ซื้อได้เฉพาะของที่ร้านนี้มี
            if (StoreHasItem(store, id))
                item.Restock();
        }

        Refresh(); // อัปเดตข้อความ "ขาด" ใหม่หลังซื้อ
    }

    bool StoreHasItem(StoreData store, string id)
    {
        if (store.sellsIds == null || store.sellsIds.Count == 0) return true; // ร้านมีทุกอย่าง
        return store.sellsIds.Contains(id);
    }

    // เผื่ออยากมีปุ่มเดียวเติมของที่ขาดทั้งหมด (ไม่สนร้าน)
    public void RestockMissing()
    {
        if (currentMenu == null) return;
        foreach (var id in currentMenu.requiredIds)
        {
            var item = selectionManager.GetItemById(id);
            if (item != null && item.isOutOfStock) item.Restock();
        }
        Refresh();
    }
}