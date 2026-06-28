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
    public bool autoStartOnCardClick = true;

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

    // ลบ Setup() ออกไปแล้ว ✅

    void Start()
    {
         foreach (Transform child in cardContainer)
        {
            if (child.GetComponent<RecipeCard>() != null)
                Destroy(child.gameObject);
        }
        startButton.interactable = false;
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButton);
        cards = new RecipeCard[recipes.Length];

        for (int i = 0; i < recipes.Length; i++)
        {
            int index = i;
            GameObject obj = Instantiate(recipeCardPrefab, cardContainer);
            RecipeCard card = obj.GetComponent<RecipeCard>();
            if (card == null)
            {
                Debug.LogError("RecipeCard component not found on Prefab!");
                return;
            }
            card.Setup(recipes[i]);
            card.GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectCard(index);
                if (autoStartOnCardClick)
                    OnStartButton();
            });
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

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found.");
            return;
        }

        GameManager.Instance.selectedRecipeIndex = index;
        GameManager.Instance.selectedRecipeName = recipes[index].name;
        GameManager.Instance.requiredIngredients = recipes[index].requiredIngredients;
    }

    public void OnStartButton()
    {
        if (selectedIndex < 0) return;
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found.");
            return;
        }
        GameManager.Instance.GoToSmartFridge();
    }
}