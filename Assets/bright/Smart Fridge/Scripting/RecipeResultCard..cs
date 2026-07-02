using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RecipeResultCard : MonoBehaviour
{
    public Image foodImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI kcalText;
    public Button selectButton;

    private Action<SmartFridgeManager.Recipe> onSelect;
    private SmartFridgeManager.Recipe recipeData;

    public void Setup(SmartFridgeManager.Recipe data, Action<SmartFridgeManager.Recipe> callback)
    {
        recipeData = data;
        foodImage.sprite = data.image;
        nameText.text = data.name;
        kcalText.text = data.kcal + " kcal";
        onSelect = callback;
        selectButton.onClick.AddListener(OnSelectClick);
    }

    void OnSelectClick()
    {
        onSelect?.Invoke(recipeData);
    }
}