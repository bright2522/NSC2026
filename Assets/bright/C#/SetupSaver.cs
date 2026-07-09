using UnityEngine;
using UnityEngine.SceneManagement;

// เก็บค่าที่ผู้เล่นเลือก (แยกจากเรื่อง UI ไม่พันกัน)
// ผูกกับปุ่มเดียวกับที่สลับหน้าได้เลย — ใส่เพิ่มอีกบรรทัดใน OnClick
public class SetupSaver : MonoBehaviour
{
    // --- บันทึกค่า (ผูกกับปุ่ม ใส่ตัวเลขในช่อง) ---

    // ปุ่มเลือกโหมด: ชีวิตจริง=0, เล่นเกม=1
    public void SaveMode(int value)
    {
        PlayerPrefs.SetInt("Setup_Mode", value);
        PlayerPrefs.Save();
        Debug.Log($"[Setup] บันทึกโหมด: {value}");
    }

    // ปุ่มเลือกวิธีเล่น: คนเดียว=0, แข่งขัน=1
    public void SavePlayType(int value)
    {
        PlayerPrefs.SetInt("Setup_PlayType", value);
        PlayerPrefs.Save();
        Debug.Log($"[Setup] บันทึกวิธีเล่น: {value}");
    }

    // ปุ่มเลือกช่วงอายุ: เด็ก=0, ผู้ใหญ่=1, สูงอายุ=2
    public void SaveAge(int value)
    {
        PlayerPrefs.SetInt("Setup_Age", value);
        PlayerPrefs.Save();
        Debug.Log($"[Setup] บันทึกอายุ: {value}");
    }

    // ปุ่มเลือกเมนู: ใส่ index ของเมนู
    public void SaveMenu(int value)
    {
        PlayerPrefs.SetInt("Setup_MenuIndex", value);
        PlayerPrefs.Save();
        Debug.Log($"[Setup] บันทึกเมนู: {value}");
    }

    // --- โหลดซีน (ผูกกับปุ่มเริ่มเกม พิมพ์ชื่อซีนในช่อง) ---
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // --- อ่านค่ากลับ (ใช้ในซีนเกม) ---
    public static int GetMode()     => PlayerPrefs.GetInt("Setup_Mode", -1);
    public static int GetPlayType() => PlayerPrefs.GetInt("Setup_PlayType", -1);
    public static int GetAge()      => PlayerPrefs.GetInt("Setup_Age", -1);
    public static int GetMenu()     => PlayerPrefs.GetInt("Setup_MenuIndex", -1);
}