using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    [Header("Main Menu Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Panel Settings (Optional)")]
    [SerializeField] private GameObject panelToClose;
    [SerializeField] private GameObject panelToOpen;

    // Go back to Main Menu scene
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Close current panel (e.g. close Settings panel)
    public void ClosePanel()
    {
        if (panelToClose != null)
            panelToClose.SetActive(false);
    }

    // Close current panel and open another (e.g. back to a previous panel)
    public void BackToPanel()
    {
        if (panelToClose != null)
            panelToClose.SetActive(false);

        if (panelToOpen != null)
            panelToOpen.SetActive(true);
    }
}
