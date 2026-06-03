// RecipeCard.cs — ติดกับ Prefab การ์ดแต่ละใบ
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeCard : MonoBehaviour
{
    public Image foodImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI kcalText;
    public TextMeshProUGUI diffText;
    public Image[] stars; // 5 star images
    public Image cardBorder;

    public Color selectedColor;
    public Color normalColor;

    public void Setup(RecipeSelectionManager.RecipeData data)
    {
        foodImage.sprite = data.foodImage;
        nameText.text = data.name;
        kcalText.text = data.kcal + " kcal";
        diffText.text = data.difficulty;

        for (int i = 0; i < stars.Length; i++)
            stars[i].color = i < data.healthStar
                ? new Color(0.91f, 0.58f, 0.43f)
                : new Color(0.85f, 0.85f, 0.85f);
    }

    public void SetSelected(bool isSelected)
    {
        cardBorder.color = isSelected ? selectedColor : normalColor;
    }
}