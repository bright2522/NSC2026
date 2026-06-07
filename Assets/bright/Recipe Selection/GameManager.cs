// GameManager.cs — Singleton เก็บข้อมูลระหว่าง Scene
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int selectedRecipeIndex;
    public string selectedAgeGroup; // "วัยรุ่น" / "วัยทำงาน" / "ผู้สูงอายุ"
    public string selectedRecipeName;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
}