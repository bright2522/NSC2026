using Pep.Recipe;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RecipeCatalogManager))]
public class RecipeCatalogManagerEditor : Editor
{
    private SerializedProperty buildIndexOnAwakeProp;
    private SerializedProperty ingredientAssetsProp;
    private SerializedProperty recipeAssetsProp;

    private string ingredientLookupId = string.Empty;
    private string recipeLookupId = string.Empty;

    private void OnEnable()
    {
        buildIndexOnAwakeProp = serializedObject.FindProperty("buildIndexOnAwake");
        ingredientAssetsProp = serializedObject.FindProperty("ingredientAssets");
        recipeAssetsProp = serializedObject.FindProperty("recipeAssets");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var manager = (RecipeCatalogManager)target;

        EditorGUILayout.LabelField("Config", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(buildIndexOnAwakeProp);
        EditorGUILayout.PropertyField(ingredientAssetsProp, true);
        EditorGUILayout.PropertyField(recipeAssetsProp, true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Ingredient Count", manager.IngredientCount.ToString());
        EditorGUILayout.LabelField("Recipe Count", manager.RecipeCount.ToString());

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Rebuild Index"))
        {
            manager.RebuildIndex();
            EditorUtility.SetDirty(manager);
        }

        ingredientLookupId = EditorGUILayout.TextField("Ingredient Id", ingredientLookupId);
        if (GUILayout.Button("Find Ingredient"))
        {
            if (manager.TryGetIngredientById(ingredientLookupId, out IngredientSO ingredient))
            {
                Selection.activeObject = ingredient;
                EditorGUIUtility.PingObject(ingredient);
            }
            else
            {
                Debug.LogWarning($"Ingredient id not found: {ingredientLookupId}");
            }
        }

        recipeLookupId = EditorGUILayout.TextField("Recipe Id", recipeLookupId);
        if (GUILayout.Button("Find Recipe"))
        {
            if (manager.TryGetRecipeById(recipeLookupId, out RecipeSO recipe))
            {
                Selection.activeObject = recipe;
                EditorGUIUtility.PingObject(recipe);
            }
            else
            {
                Debug.LogWarning($"Recipe id not found: {recipeLookupId}");
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
