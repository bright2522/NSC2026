using Pep.Core;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameStateMachine))]
public class GameStateMachineEditor : Editor
{
    private SerializedProperty initialStateProp;
    private SerializedProperty lockStateChangesProp;
    private string reasonInput = "Editor Debug";
    private PepGameState targetState = PepGameState.Boot;

    private void OnEnable()
    {
        initialStateProp = serializedObject.FindProperty("initialState");
        lockStateChangesProp = serializedObject.FindProperty("lockStateChanges");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var machine = (GameStateMachine)target;

        EditorGUILayout.LabelField("Config", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(initialStateProp);
        EditorGUILayout.PropertyField(lockStateChangesProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Current State", machine.CurrentState.ToString());
        EditorGUILayout.LabelField("Previous State", machine.PreviousState.ToString());
        EditorGUILayout.LabelField("Locked", machine.IsStateChangeLocked ? "Yes" : "No");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Actions", EditorStyles.boldLabel);
        reasonInput = EditorGUILayout.TextField("Reason", reasonInput);
        targetState = (PepGameState)EditorGUILayout.EnumPopup("Target State", targetState);

        using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
        {
            if (GUILayout.Button("Change State"))
            {
                machine.TryChangeState(targetState, reasonInput);
                EditorUtility.SetDirty(machine);
            }

            if (GUILayout.Button("Force Change State"))
            {
                machine.TryChangeState(targetState, reasonInput, true);
                EditorUtility.SetDirty(machine);
            }

            if (GUILayout.Button("Toggle Lock"))
            {
                machine.SetStateLock(!machine.IsStateChangeLocked);
                EditorUtility.SetDirty(machine);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
