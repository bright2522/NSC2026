using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int selectedRecipeIndex;
    public string selectedAgeGroup;
    public string selectedRecipeName;
    public List<string> requiredIngredients = new List<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void GoToSmartFridge()
    {
        SceneManager.LoadScene("SmartFridge");
    }

    public void GoToCooking()
    {
        SceneManager.LoadScene("CookingGameplay");
    }
}