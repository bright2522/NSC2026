using Pep.Core;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerDataManager))]
public class PlayerDataManagerEditor : Editor
{
    private SerializedProperty loadOnAwakeProp;
    private SerializedProperty currentDataProp;
    private bool showOwnedIngredients = true;
    private bool showUnlockedRecipes = true;

    private void OnEnable()
    {
        loadOnAwakeProp = serializedObject.FindProperty("loadOnAwake");
        currentDataProp = serializedObject.FindProperty("currentData");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Config", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(loadOnAwakeProp);
        EditorGUILayout.PropertyField(currentDataProp, true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
        DrawRuntimeSection((PlayerDataManager)target);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Actions", EditorStyles.boldLabel);
        DrawActionSection((PlayerDataManager)target);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRuntimeSection(PlayerDataManager manager)
    {
        var data = manager.CurrentData;
        if (data == null)
        {
            EditorGUILayout.HelpBox("No current data.", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField("Player Id", data.playerId ?? string.Empty);
        EditorGUILayout.LabelField("Player Name", data.playerName ?? string.Empty);
        EditorGUILayout.LabelField("Total Score", data.totalScore.ToString());
        EditorGUILayout.LabelField("Level", data.level.ToString());
        EditorGUILayout.LabelField("Last Updated Utc", data.lastUpdatedUtc ?? string.Empty);

        showOwnedIngredients = EditorGUILayout.Foldout(showOwnedIngredients, $"Owned Ingredients ({data.ownedIngredients?.Count ?? 0})", true);
        if (showOwnedIngredients && data.ownedIngredients != null)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < data.ownedIngredients.Count; i++)
            {
                EditorGUILayout.LabelField($"{i + 1}.", data.ownedIngredients[i]);
            }
            EditorGUI.indentLevel--;
        }

        showUnlockedRecipes = EditorGUILayout.Foldout(showUnlockedRecipes, $"Unlocked Recipes ({data.unlockedRecipes?.Count ?? 0})", true);
        if (showUnlockedRecipes && data.unlockedRecipes != null)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < data.unlockedRecipes.Count; i++)
            {
                EditorGUILayout.LabelField($"{i + 1}.", data.unlockedRecipes[i]);
            }
            EditorGUI.indentLevel--;
        }
    }

    private void DrawActionSection(PlayerDataManager manager)
    {
        using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
        {
            if (GUILayout.Button("Load"))
            {
                manager.Load();
                EditorUtility.SetDirty(manager);
            }

            if (GUILayout.Button("Save"))
            {
                manager.Save();
                EditorUtility.SetDirty(manager);
            }

            if (GUILayout.Button("Clear Save"))
            {
                manager.ClearSave();
                EditorUtility.SetDirty(manager);
            }
        }
    }
}
