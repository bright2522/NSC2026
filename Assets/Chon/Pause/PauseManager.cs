using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("--- UI Panel ---")]
    [SerializeField] private GameObject pauseMenuPanel; // ลากหน้าต่าง Pause_Panel มาใส่ช่องนี้ (ถ้ามี)

    private bool isPaused = false; // ตัวแปรเช็กว่าตอนนี้หยุดเกมอยู่ไหม

    void Update()
    {
        // สามารถกดปุ่ม Esc หรือ ปุ่ม P บนคีย์บอร์ดเพื่อหยุด/เล่นต่อได้ด้วย
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    // ฟังก์ชันนี้ไว้สลับสถานะ (ใช้ผูกกับปุ่ม UI ได้)
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f; // 🛑 หยุดเวลาในเกมทั้งหมด (รวมถึง Time.deltaTime ใน TimerSystem)
        isPaused = true;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true); // เปิดหน้าต่างหยุดเกม
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // ▶️ ให้เวลาเดินตามปกติ
        isPaused = false;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false); // ปิดหน้าต่างหยุดเกม
        }
    }

    // ฟังก์ชันเพิ่มเติม: สำหรับปุ่มกดกลับหน้าหลักหรือเริ่มใหม่
    public void RestartGame()
    {
        Time.timeScale = 1f; // **สำคัญมาก** ต้องคืนค่าเวลาก่อนเปลี่ยนซีน/เริ่มใหม่เสมอ
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}