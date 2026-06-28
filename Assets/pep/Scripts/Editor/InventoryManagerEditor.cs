using Pep.SmartFridge;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InventoryManager))]
public class InventoryManagerEditor : Editor
{
    private SerializedProperty initialInventoryProp;
    private string ingredientIdInput = string.Empty;
    private int amountInput = 1;
    private bool showSnapshot = true;

    private void OnEnable()
    {
        initialInventoryProp = serializedObject.FindProperty("initialInventory");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var manager = (InventoryManager)target;

        EditorGUILayout.LabelField("Config", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(initialInventoryProp, true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
        DrawRuntimeSection(manager);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Actions", EditorStyles.boldLabel);
        DrawDebugActions(manager);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRuntimeSection(InventoryManager manager)
    {
        var snapshot = manager.GetSnapshot();
        showSnapshot = EditorGUILayout.Foldout(showSnapshot, $"Stock Snapshot ({snapshot.Count})", true);
        if (!showSnapshot)
        {
            return;
        }

        EditorGUI.indentLevel++;
        if (snapshot.Count == 0)
        {
            EditorGUILayout.LabelField("Empty");
        }
        else
        {
            for (int i = 0; i < snapshot.Count; i++)
            {
                var entry = snapshot[i];
                if (entry == null) continue;
                EditorGUILayout.LabelField(entry.ingredientId, entry.amount.ToString());
            }
        }
        EditorGUI.indentLevel--;
    }

    private void DrawDebugActions(InventoryManager manager)
    {
        ingredientIdInput = EditorGUILayout.TextField("Ingredient Id", ingredientIdInput);
        amountInput = Mathf.Max(1, EditorGUILayout.IntField("Amount", amountInput));

        using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying || string.IsNullOrWhiteSpace(ingredientIdInput)))
        {
            if (GUILayout.Button("Add Ingredient"))
            {
                manager.AddIngredient(ingredientIdInput, amountInput);
                EditorUtility.SetDirty(manager);
            }

            if (GUILayout.Button("Consume Ingredient"))
            {
                manager.ConsumeIngredient(ingredientIdInput, amountInput);
                EditorUtility.SetDirty(manager);
            }
        }
    }
}
