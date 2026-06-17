using Pep.Scoring;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScoringManager))]
public class ScoringManagerEditor : Editor
{
    private SerializedProperty minScoreProp;
    private SerializedProperty maxScoreProp;
    private SerializedProperty clearOnAwakeProp;

    private string sourceInput = "Editor";
    private string stepNameInput = "Manual Step";
    private float scoreInput = 50f;
    private bool showSteps = true;

    private void OnEnable()
    {
        minScoreProp = serializedObject.FindProperty("minScore");
        maxScoreProp = serializedObject.FindProperty("maxScore");
        clearOnAwakeProp = serializedObject.FindProperty("clearOnAwake");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var manager = (ScoringManager)target;

        EditorGUILayout.LabelField("Config", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(minScoreProp);
        EditorGUILayout.PropertyField(maxScoreProp);
        EditorGUILayout.PropertyField(clearOnAwakeProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
        DrawRuntimeSection(manager);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Actions", EditorStyles.boldLabel);
        DrawDebugActions(manager);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRuntimeSection(ScoringManager manager)
    {
        EditorGUILayout.LabelField("Total Score", manager.TotalScore.ToString("0.##"));
        EditorGUILayout.LabelField("Step Count", manager.StepCount.ToString());
        EditorGUILayout.LabelField("Average Score", manager.AverageScore.ToString("0.##"));

        var steps = manager.GetStepScoreSnapshot();
        showSteps = EditorGUILayout.Foldout(showSteps, $"Step Scores ({steps.Count})", true);
        if (!showSteps)
        {
            return;
        }

        EditorGUI.indentLevel++;
        if (steps.Count == 0)
        {
            EditorGUILayout.LabelField("No step scores yet.");
        }
        else
        {
            for (int i = 0; i < steps.Count; i++)
            {
                var entry = steps[i];
                if (entry == null) continue;
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Source", entry.source ?? string.Empty);
                EditorGUILayout.LabelField("Step Name", entry.stepName ?? string.Empty);
                EditorGUILayout.LabelField("Score", entry.score.ToString("0.##"));
                EditorGUILayout.LabelField("Time", entry.time.ToString("0.##"));
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUI.indentLevel--;
    }

    private void DrawDebugActions(ScoringManager manager)
    {
        sourceInput = EditorGUILayout.TextField("Source", sourceInput);
        stepNameInput = EditorGUILayout.TextField("Step Name", stepNameInput);
        scoreInput = EditorGUILayout.FloatField("Score", scoreInput);

        using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
        {
            if (GUILayout.Button("Report Step Score"))
            {
                manager.ReportStepScore(sourceInput, stepNameInput, scoreInput);
                EditorUtility.SetDirty(manager);
            }

            if (GUILayout.Button("Reset Scores"))
            {
                manager.ResetScores();
                EditorUtility.SetDirty(manager);
            }

            if (GUILayout.Button("Complete Recipe And Log Average"))
            {
                float finalAverage = manager.CompleteRecipeAndGetFinalAverage();
                Debug.Log($"ScoringManager final average: {finalAverage:0.##}");
                EditorUtility.SetDirty(manager);
            }
        }
    }
}
