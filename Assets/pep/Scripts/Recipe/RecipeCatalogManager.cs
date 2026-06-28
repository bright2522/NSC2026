using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pep.Recipe
{
    public class RecipeCatalogManager : MonoBehaviour
    {
        [SerializeField] private bool buildIndexOnAwake = true;
        [SerializeField] private List<IngredientSO> ingredientAssets = new List<IngredientSO>();
        [SerializeField] private List<RecipeSO> recipeAssets = new List<RecipeSO>();

        private readonly Dictionary<string, IngredientSO> ingredientById = new Dictionary<string, IngredientSO>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RecipeSO> recipeById = new Dictionary<string, RecipeSO>(StringComparer.OrdinalIgnoreCase);

        public int IngredientCount => ingredientById.Count;
        public int RecipeCount => recipeById.Count;

        private void Awake()
        {
            if (buildIndexOnAwake)
            {
                RebuildIndex();
            }
        }

        private void OnValidate()
        {
            RebuildIndex();
        }

        public void RebuildIndex()
        {
            ingredientById.Clear();
            recipeById.Clear();

            for (int i = 0; i < ingredientAssets.Count; i++)
            {
                IngredientSO item = ingredientAssets[i];
                if (item == null || string.IsNullOrWhiteSpace(item.Id)) continue;
                ingredientById[item.Id] = item;
            }

            for (int i = 0; i < recipeAssets.Count; i++)
            {
                RecipeSO item = recipeAssets[i];
                if (item == null || string.IsNullOrWhiteSpace(item.Id)) continue;
                recipeById[item.Id] = item;
            }
        }

        public bool TryGetIngredientById(string ingredientId, out IngredientSO ingredient)
        {
            if (string.IsNullOrWhiteSpace(ingredientId))
            {
                ingredient = null;
                return false;
            }

            return ingredientById.TryGetValue(ingredientId, out ingredient);
        }

        public bool TryGetRecipeById(string recipeId, out RecipeSO recipe)
        {
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                recipe = null;
                return false;
            }

            return recipeById.TryGetValue(recipeId, out recipe);
        }

        public IngredientSO GetIngredientById(string ingredientId)
        {
            TryGetIngredientById(ingredientId, out IngredientSO result);
            return result;
        }

        public RecipeSO GetRecipeById(string recipeId)
        {
            TryGetRecipeById(recipeId, out RecipeSO result);
            return result;
        }

        public List<IngredientSO> ResolveIngredientAssets(IReadOnlyList<string> ingredientIds)
        {
            var list = new List<IngredientSO>();
            if (ingredientIds == null) return list;

            for (int i = 0; i < ingredientIds.Count; i++)
            {
                string id = ingredientIds[i];
                if (TryGetIngredientById(id, out IngredientSO ingredient))
                {
                    list.Add(ingredient);
                }
            }

            return list;
        }

        public List<string> GetIngredientIdList()
        {
            var ids = new List<string>(ingredientById.Count);
            foreach (var pair in ingredientById)
            {
                ids.Add(pair.Key);
            }
            return ids;
        }

        public List<string> GetRecipeIdList()
        {
            var ids = new List<string>(recipeById.Count);
            foreach (var pair in recipeById)
            {
                ids.Add(pair.Key);
            }
            return ids;
        }
    }
}
