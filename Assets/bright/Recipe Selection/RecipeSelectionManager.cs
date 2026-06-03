using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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
        public string difficulty; // "ง่าย" / "ปานกลาง" / "ยาก"
        public int healthStar;   // 1-5
        public Sprite foodImage;
    }

    public RecipeData[] recipes;

    void Start()
    {
        startButton.interactable = false;
        cards = new RecipeCard[recipes.Length];

        for (int i = 0; i < recipes.Length; i++)
        {
            int index = i; // capture for lambda
            GameObject obj = Instantiate(recipeCardPrefab, cardContainer);
            RecipeCard card = obj.GetComponent<RecipeCard>();
            card.Setup(recipes[i]);
            card.GetComponent<Button>().onClick.AddListener(() => SelectCard(index));
            cards[i] = card;
        }
    }

    void SelectCard(int index)
    {
        // deselect เก่า
        if (selectedIndex >= 0)
            cards[selectedIndex].SetSelected(false);

        selectedIndex = index;
        cards[index].SetSelected(true);
        selectedLabel.text = "เลือก: " + recipes[index].name;
        startButton.interactable = true;

        // บันทึกเมนูที่เลือกไว้ใน GameManager
        GameManager.Instance.selectedRecipeIndex = index;
    }

    public void OnStartButton()
    {
        if (selectedIndex < 0) return;
        SceneManager.LoadScene("CookingGameplay");
    }
}