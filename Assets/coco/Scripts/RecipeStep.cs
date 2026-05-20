using System;
using UnityEngine;

namespace CookingGame
{
    public enum MinigameType
    {
        Chopping,
        Stirring,
        Grilling,
        Seasoning,
        Pounding
    }

    [Serializable]
    public class RecipeStep
    {
        [Header("Instruction")]
        [Tooltip("Instruction shown to the player in Thai")]
        public string instructionThai;

        [Header("Minigame Configuration")]
        public MinigameType minigameType;
        public float timeLimit = 15f;
        public string targetIngredientName;

        [Header("Visual Prefabs (Optional references)")]
        [Tooltip("The raw or starting state prefab name or reference")]
        public string ingredientPrefabName;
        [Tooltip("The cooked or finished state prefab name or reference")]
        public string resultPrefabName;
        [Tooltip("The tool or utensil used in this step (e.g. knife, pot, mortar)")]
        public string toolPrefabName;

        public RecipeStep(string instructionThai, MinigameType minigameType, float timeLimit, string targetIngredientName, string ingredientPrefabName = "", string resultPrefabName = "", string toolPrefabName = "")
        {
            this.instructionThai = instructionThai;
            this.minigameType = minigameType;
            this.timeLimit = timeLimit;
            this.targetIngredientName = targetIngredientName;
            this.ingredientPrefabName = ingredientPrefabName;
            this.resultPrefabName = resultPrefabName;
            this.toolPrefabName = toolPrefabName;
        }
    }
}
