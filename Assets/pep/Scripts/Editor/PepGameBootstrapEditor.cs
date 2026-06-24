using Pep.Core;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PepGameBootstrap))]
public class PepGameBootstrapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var bootstrap = (PepGameBootstrap)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Running", bootstrap.IsRunning ? "Yes" : "No");
        EditorGUILayout.LabelField("Current Flow Step", bootstrap.CurrentFlowStep);
        EditorGUILayout.LabelField("Selected Recipe", bootstrap.SelectedRecipeId);

        using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Start Flow"))
            {
                bootstrap.StartFlow();
                EditorUtility.SetDirty(bootstrap);
            }

            if (GUILayout.Button("Restart Flow"))
            {
                bootstrap.RestartFlow();
                EditorUtility.SetDirty(bootstrap);
            }

            if (GUILayout.Button("Skip Current Step"))
            {
                bootstrap.RequestSkipCurrentStep();
                EditorUtility.SetDirty(bootstrap);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Force Boot")) bootstrap.ForceState(PepGameState.Boot);
            if (GUILayout.Button("Force Cooking")) bootstrap.ForceState(PepGameState.Cooking);
            if (GUILayout.Button("Force Result")) bootstrap.ForceState(PepGameState.Result);
            EditorGUILayout.EndHorizontal();
        }
    }
}
