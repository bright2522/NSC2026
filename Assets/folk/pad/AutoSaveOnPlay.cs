#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class AutoSaveOnPlay
{
    static AutoSaveOnPlay()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        // ทำการเซฟอัตโนมัติทันทีก่อนที่หน้าต่างเกมจะรัน (ก่อนจะเกิดการค้าง)
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            Debug.Log("💾 [Auto-Save] กำลังบันทึก Scene และ Assets ทั้งหมดให้ก่อนรันเกม...");
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
        }
    }
}
#endif