using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneName = "GameScene";

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }

    // Optional: Load scene by build index instead
    public void LoadSceneByIndex(int index)
    {
        SceneManager.LoadScene(index);
    }
}