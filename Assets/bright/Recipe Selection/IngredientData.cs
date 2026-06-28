using UnityEngine;

[System.Serializable]
public class IngredientData
{
    public string id;
    public string itemName;
    public int price = 0;

    [Range(0f, 1f)]
    public float outOfStockChance = 0.3f;

    [HideInInspector] public bool isOutOfStock;

    public void RollStock()
    {
        isOutOfStock = Random.value < outOfStockChance;
    }
}