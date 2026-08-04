using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadCompetitionScene()
    {
        SceneManager.LoadScene("CreateRoommain");
    }
}