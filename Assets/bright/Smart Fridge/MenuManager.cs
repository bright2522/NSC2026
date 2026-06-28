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
    public TMP_Text missingText;          // ขึ้นว่าอะไรหมดบ้าง

    [Tooltip("ลาก TMP Text ของแต่ละกล่องร้านมาใส่ — ร้านใกล้สุดลงช่องแรก ไล่ไปช่องถัดไป")]
    public List<TMP_Text> storeTexts = new List<TMP_Text>(); // ป้ายร้าน หลายช่อง

    private MenuData currentMenu;

    public void OnMenuClicked(int menuIndex)
    {
        if (menuIndex < 0 || menuIndex >= menus.Count) return;
        currentMenu = menus[menuIndex];
        Refresh();
    }

    void Refresh()
    {
        if (currentMenu == null) return;

        // หาวัตถุดิบที่ขาด (หมด)
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

    // เขียนชื่อร้านลงแต่ละกล่อง เรียงใกล้ -> ไกล
    void ShowStores()
    {
        var ordered = stores.OrderBy(s => s.distanceMeters).ToList();

        for (int i = 0; i < storeTexts.Count; i++)
        {
            if (storeTexts[i] == null) continue;

            if (i < ordered.Count)
            {
                var s = ordered[i];
                storeTexts[i].text = $"{s.storeName}  {FormatDistance(s.distanceMeters)}";
                storeTexts[i].gameObject.SetActive(true);
            }
            else
            {
                // ถ้ามีกล่องมากกว่าจำนวนร้าน ช่องเกินก็ซ่อนไว้
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