using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeCard : MonoBehaviour
{
    public Image foodImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI kcalText;
    public TextMeshProUGUI diffText;
    public Image[] stars;
    public Image cardBorder;

    public Color selectedColor;
    public Color normalColor;

    public void Setup(RecipeSelectionManager.RecipeData data)
    {
        if (foodImage != null) foodImage.sprite = data.foodImage;
        if (nameText != null)  nameText.text = data.name;
        if (kcalText != null)  kcalText.text = data.kcal + " kcal";
        if (diffText != null)  diffText.text = data.difficulty;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
                stars[i].color = i < data.healthStar
                    ? new Color(0.91f, 0.58f, 0.43f)
                    : new Color(0.85f, 0.85f, 0.85f);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (cardBorder != null)
            cardBorder.color = isSelected ? selectedColor : normalColor;
    }
}