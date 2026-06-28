using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public void GoToVersusScene()
    {
        SceneManager.LoadScene("VersusScene");
    }
}