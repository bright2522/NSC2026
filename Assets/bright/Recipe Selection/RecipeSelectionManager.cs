using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RecipeSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject recipeCardPrefab;
    public Transform cardContainer;
    public Button startButton;
    public TextMeshProUGUI selectedLabel;

    private int selectedIndex = -1;
    private RecipeCard[] cards;

    [System.Serializable]
    public class RecipeData
    {
        public string name;
        public int kcal;
        public string difficulty;
        public int healthStar;
        public Sprite foodImage;
        public List<string> requiredIngredients;
    }

    public RecipeData[] recipes;
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

    void Start()
    {
        startButton.interactable = false;
        cards = new RecipeCard[recipes.Length];

        for (int i = 0; i < recipes.Length; i++)
        {
            Debug.Log($"Recipe[{i}] name={recipes[i].name} kcal={recipes[i].kcal} image={recipes[i].foodImage}");
            int index = i;
            GameObject obj = Instantiate(recipeCardPrefab, cardContainer);
            Debug.Log("Instantiated: " + obj.name);
            RecipeCard card = obj.GetComponent<RecipeCard>();
            Debug.Log("RecipeCard component: " + card);
            if (card == null)
            {
                Debug.LogError("RecipeCard component not found on Prefab!");
                return;
            }
            card.Setup(recipes[i]);
            card.GetComponent<Button>().onClick.AddListener(() => SelectCard(index));
            cards[i] = card;
        }
    }

    void SelectCard(int index)
    {
        if (selectedIndex >= 0)
            cards[selectedIndex].SetSelected(false);

        selectedIndex = index;
        cards[index].SetSelected(true);
        selectedLabel.text = "เลือก: " + recipes[index].name;
        startButton.interactable = true;

        GameManager.Instance.selectedRecipeIndex = index;
        GameManager.Instance.selectedRecipeName = recipes[index].name;
        GameManager.Instance.requiredIngredients = recipes[index].requiredIngredients;
    }

    public void OnStartButton()
    {
        if (selectedIndex < 0) return;
        GameManager.Instance.GoToSmartFridge();
    }
}