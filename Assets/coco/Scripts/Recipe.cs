using System.Collections.Generic;
using UnityEngine;

namespace CookingGame
{
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking Game/Recipe", order = 1)]
    public class Recipe : ScriptableObject
    {
        [Header("Recipe Information")]
        public string recipeNameThai;
        public string recipeNameEnglish;
        
        [TextArea(3, 10)]
        public string description;

        [Header("Steps")]
        public List<RecipeStep> steps = new List<RecipeStep>();

        [Header("Visual Asset")]
        public string finalDishPrefabName;

        // Factory method to allow programmatic creation without file saving
        public static Recipe CreateInstance(string nameThai, string nameEnglish, string desc, List<RecipeStep> stepsList, string finalDish = "")
        {
            Recipe recipe = ScriptableObject.CreateInstance<Recipe>();
            recipe.recipeNameThai = nameThai;
            recipe.recipeNameEnglish = nameEnglish;
            recipe.description = desc;
            recipe.steps = stepsList;
            recipe.finalDishPrefabName = finalDish;
            return recipe;
        }
    }
}
