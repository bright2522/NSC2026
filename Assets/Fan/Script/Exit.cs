using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExitGame : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // Stops play mode in Editor
#else
        Application.Quit(); // Quits the built game
#endif
    }
}
