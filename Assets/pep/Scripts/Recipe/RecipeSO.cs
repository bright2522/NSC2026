using System.Collections.Generic;
using UnityEngine;

namespace Pep.Recipe
{
    [CreateAssetMenu(fileName = "Recipe_", menuName = "PEP/Recipe/Recipe SO")]
    public class RecipeSO : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private int kcal;
        [SerializeField] private int difficultyLevel = 1;
        [SerializeField] private Sprite thumbnail;
        [SerializeField] private List<string> requiredIngredientIds = new List<string>();

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public int Kcal => kcal;
        public int DifficultyLevel => difficultyLevel;
        public Sprite Thumbnail => thumbnail;
        public IReadOnlyList<string> RequiredIngredientIds => requiredIngredientIds;
    }
}
