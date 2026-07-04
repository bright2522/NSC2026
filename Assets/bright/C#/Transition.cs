using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition : MonoBehaviour
{
    public string SceneName;

    public void LoadScene()
    {
                 SceneManager.LoadScene(SceneName);

    }

    public void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void DestroyMe()
    {
        Destroy(gameObject);
    }

    public void SetScene(string sceneName)
    {
        SceneName = sceneName;
    }
}