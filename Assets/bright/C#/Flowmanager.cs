using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// คุม flow ทั้งหมด — ลากหน้า UI ใส่ Inspector ครั้งเดียว
/// ปุ่มแค่เลือกฟังก์ชัน ไม่ต้องลากหน้าใส่ทุกปุ่ม
///
/// flow:
///   เลือกอายุ -> เลือกโหมด
///   โหมดชีวิตจริง -> UI ชีวิตจริง (จบ)
///   โหมดเล่นเกม   -> เลือกคนเดียว/แข่งขัน
///        คนเดียว   -> UI เล่นคนเดียว (จบ)
///        แข่งขัน   -> เลือกสร้าง/เข้าร่วมห้อง
///             สร้างห้อง    -> UI สร้างห้อง
///             เข้าร่วมห้อง -> UI เข้าร่วมห้อง
/// </summary>
public class FlowManager : MonoBehaviour
{
    [Header("ซีนของโหมดชีวิตจริง (พิมพ์ชื่อซีน)")]
    public string realLifeSceneName = "";

    [Header("หน้า UI ทั้งหมด (ลากใส่ครั้งเดียว)")]
    public GameObject pageAge;           // หน้าเลือกอายุ (หน้าแรก)
    public GameObject pageMode;          // หน้าเลือกโหมด (ชีวิตจริง/เล่นเกม)
    public GameObject pagePlayType;      // หน้าเลือก คนเดียว/แข่งขัน
    public GameObject pageMenu;          // หน้าเลือกเมนู (ListMenu) — มาหลังกดเล่นคนเดียว
    public GameObject pageRoomChoice;    // หน้าเลือก สร้างห้อง/เข้าร่วมห้อง
    public GameObject pageCreateRoom;    // UI สร้างห้อง
    public GameObject pageJoinRoom;      // UI เข้าร่วมห้อง

    void Start()
    {
        ShowOnly(pageAge); // เริ่มที่หน้าแรก
    }

    // ---------- ปุ่มเลือกอายุ (ใส่เลข 0=เด็ก 1=ผู้ใหญ่ 2=สูงอายุ) ----------
    public void ChooseAge(int age)
    {
        PlayerPrefs.SetInt("Setup_Age", age);
        Debug.Log($"[Flow] เลือกอายุ: {age}");
        ShowOnly(pageMode);
    }

    // ---------- ปุ่มเลือกโหมด ----------
    public void GoRealLife()          // ปุ่ม "โหมดชีวิตจริง" -> เปลี่ยนซีนเลย
    {
        PlayerPrefs.SetInt("Setup_Mode", 0);
        PlayerPrefs.Save();

        if (string.IsNullOrEmpty(realLifeSceneName))
        {
            Debug.LogError("[Flow] ยังไม่ได้พิมพ์ชื่อซีนในช่อง Real Life Scene Name");
            return;
        }
        SceneManager.LoadScene(realLifeSceneName);
    }

    public void GoGameMode()          // ปุ่ม "โหมดเล่นเกม"
    {
        PlayerPrefs.SetInt("Setup_Mode", 1);
        ShowOnly(pagePlayType);
    }

    // ---------- ปุ่มเลือกวิธีเล่น ----------
    public void GoSolo()              // ปุ่ม "เล่นคนเดียว" -> ไปหน้าเลือกเมนู
    {
        PlayerPrefs.SetInt("Setup_PlayType", 0);
        ShowOnly(pageMenu);
    }

    public void GoCompetition()
    {
        PlayerPrefs.SetInt("Setup_PlayType", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("CreateRoommain");
    }

    // ---------- ปุ่มเลือกห้อง ----------
    public void GoCreateRoom()        // ปุ่ม "สร้างห้อง"
    {
        ShowOnly(pageCreateRoom);
    }

    public void GoJoinRoom()          // ปุ่ม "เข้าร่วมห้อง"
    {
        ShowOnly(pageJoinRoom);
    }

    // ---------- ปุ่มย้อนกลับ (ใช้ได้ทุกหน้า) ----------
    public void BackToAge()        => ShowOnly(pageAge);
    public void BackToMode()       => ShowOnly(pageMode);
    public void BackToPlayType()   => ShowOnly(pagePlayType);
    public void BackToRoomChoice() => ShowOnly(pageRoomChoice);

    // ---------- ปุ่มเริ่มเกมจริง (พิมพ์ชื่อซีนในช่อง) ----------
    public void LoadScene(string sceneName)
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene(sceneName);
    }

    // ---------- ตัวจัดการหน้า: โชว์อันเดียว ปิดที่เหลือ ----------
    void ShowOnly(GameObject target)
    {
        SetActive(pageAge,        pageAge        == target);
        SetActive(pageMode,       pageMode       == target);
        SetActive(pagePlayType,   pagePlayType   == target);
        SetActive(pageMenu,       pageMenu       == target);
        SetActive(pageRoomChoice, pageRoomChoice == target);
        SetActive(pageCreateRoom, pageCreateRoom == target);
        SetActive(pageJoinRoom,   pageJoinRoom   == target);
    }

    void SetActive(GameObject page, bool on)
    {
        if (page != null) page.SetActive(on);
    }
}