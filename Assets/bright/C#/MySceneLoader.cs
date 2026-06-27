using UnityEngine;
using UnityEngine.SceneManagement;

// สคริปต์เปลี่ยนซีน (เปลี่ยนชื่อเป็น MySceneLoader กันชนกับของเดิมในโปรเจกต์)
public class MySceneLoader : MonoBehaviour
{
    // เปลี่ยนไปซีนตามชื่อ
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // เปลี่ยนไปซีนตามลำดับใน Build Settings (0,1,2,...)
    public void LoadSceneByIndex(int index)
    {
        SceneManager.LoadScene(index);
    }

    // ไปซีนถัดไปใน Build Settings
    public void LoadNextScene()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(next);
    }

    // โหลดซีนเดิมใหม่
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ออกจากเกม
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}