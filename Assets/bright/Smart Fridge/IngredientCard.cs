using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class IngredientCard : MonoBehaviour
{
    public Image foodImage;
    public TextMeshProUGUI nameText;
    public GameObject selectedOverlay;

    private bool isSelected = false;
    private string ingredientName;
    private Action<string, bool> onToggle;

    public void Setup(SmartFridgeManager.Ingredient data, Action<string, bool> callback)
    {
        foodImage.sprite = data.image;
        nameText.text = data.name;
        ingredientName = data.name;
        onToggle = callback;
        selectedOverlay.SetActive(false);

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        isSelected = !isSelected;
        selectedOverlay.SetActive(isSelected);
        onToggle?.Invoke(ingredientName, isSelected);
    }
}