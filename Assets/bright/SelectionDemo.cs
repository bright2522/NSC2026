using System.Collections.Generic;
using UnityEngine;

public class SelectionDemo : MonoBehaviour
{
    public MultiSelectManager manager;

    public List<IngredientData> ingredients = new List<IngredientData>
    {
        new IngredientData { id = "01", itemName = "ไส้กรอก",   price = 45, outOfStockChance = 0.3f },
        new IngredientData { id = "02", itemName = "ไข่ไก่",    price = 5,  outOfStockChance = 0.3f },
        new IngredientData { id = "03", itemName = "หัวหอม",    price = 10, outOfStockChance = 0.3f },
        new IngredientData { id = "04", itemName = "น้ำมัน",    price = 30, outOfStockChance = 0.3f },
        new IngredientData { id = "05", itemName = "มะเขือเทศ", price = 15, outOfStockChance = 0.3f },
        new IngredientData { id = "06", itemName = "แครอท",     price = 12, outOfStockChance = 0.3f },
    };

    private bool built = false; // กันไม่ให้สุ่ม/สร้างซ้ำ

    // เรียกจาก event "เปิดตู้เย็นครั้งแรก" ของ FridgeTouchController
    public void BuildIngredients()
    {
        if (built) return;   // สุ่มไปแล้ว ไม่ทำซ้ำ
        built = true;

        // สุ่มว่าอันไหน "หมด" — ทำครั้งเดียวตลอดเกม
        foreach (var item in ingredients)
            item.RollStock();

        manager.Populate(ingredients);
        manager.OnSelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(List<SelectableItem> selectedItems)
    {
        Debug.Log($"เลือกอยู่ {selectedItems.Count} อัน");
    }
}