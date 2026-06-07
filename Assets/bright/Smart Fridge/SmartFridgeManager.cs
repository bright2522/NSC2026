using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SmartFridgeManager : MonoBehaviour
{
    [Header("Ingredient UI")]
    public GameObject ingredientCardPrefab;
    public Transform ingredientContainer;
    public Button confirmButton;

    [Header("Result UI")]
    public GameObject resultPanel;
    public GameObject recipeResultCardPrefab;
    public Transform resultContainer;

    // ข้อมูลวัตถุดิบทั้งหมด
    [System.Serializable]
    public class Ingredient
    {
        public string name;
        public Sprite image;
    }

    // ข้อมูลเมนูและวัตถุดิบที่ต้องใช้
    [System.Serializable]
    public class Recipe
    {
        public string name;
        public Sprite image;
        public string[] requiredIngredients; // ชื่อวัตถุดิบที่ต้องใช้
        public int kcal;
    }

    public Ingredient[] allIngredients;
    public Recipe[] allRecipes;

    private List<string> selectedIngredients = new List<string>();

    void Start()
    {
        resultPanel.SetActive(false);
        SpawnIngredientCards();
    }

    void SpawnIngredientCards()
    {
        foreach (var ingredient in allIngredients)
        {
            GameObject obj = Instantiate(ingredientCardPrefab, ingredientContainer);
            IngredientCard card = obj.GetComponent<IngredientCard>();
            card.Setup(ingredient, OnIngredientToggle);
        }
    }

    void OnIngredientToggle(string ingredientName, bool isSelected)
    {
        if (isSelected)
            selectedIngredients.Add(ingredientName);
        else
            selectedIngredients.Remove(ingredientName);
    }

    public void OnConfirmButton()
    {
        if (selectedIngredients.Count == 0) return;

        // Greedy Algorithm หาเมนูที่ทำได้
        List<Recipe> matchedRecipes = FindMatchingRecipes();

        ShowResults(matchedRecipes);
    }

    List<Recipe> FindMatchingRecipes()
    {
        List<Recipe> results = new List<Recipe>();

        foreach (var recipe in allRecipes)
        {
            int matchCount = 0;
            foreach (var req in recipe.requiredIngredients)
            {
                if (selectedIngredients.Contains(req))
                    matchCount++;
            }

            // ถ้ามีวัตถุดิบครบ 70% ขึ้นไปถือว่าทำได้
            float matchRatio = (float)matchCount / recipe.requiredIngredients.Length;
            if (matchRatio >= 0.7f)
                results.Add(recipe);
        }

        // เรียงจากที่ match มากสุดก่อน (Greedy)
        results.Sort((a, b) => {
            int countA = CountMatch(a);
            int countB = CountMatch(b);
            return countB.CompareTo(countA);
        });

        return results;
    }

    int CountMatch(Recipe recipe)
    {
        int count = 0;
        foreach (var req in recipe.requiredIngredients)
            if (selectedIngredients.Contains(req))
                count++;
        return count;
    }

    void ShowResults(List<Recipe> recipes)
    {
        // ล้างผลเก่า
        foreach (Transform child in resultContainer)
            Destroy(child.gameObject);

        resultPanel.SetActive(true);

        if (recipes.Count == 0)
        {
            // ไม่มีเมนูที่ทำได้ — แสดงข้อความแนะนำ
            // TODO: แสดงร้านค้าใกล้เคียง
            return;
        }

        foreach (var recipe in recipes)
        {
            GameObject obj = Instantiate(recipeResultCardPrefab, resultContainer);
            RecipeResultCard card = obj.GetComponent<RecipeResultCard>();
            card.Setup(recipe, OnSelectRecipe);
        }
    }

    void OnSelectRecipe(Recipe recipe)
    {
        GameManager.Instance.selectedRecipeName = recipe.name;
        UnityEngine.SceneManagement.SceneManager.LoadScene("CookingGameplay");
    }
}