using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// คุม flow การเลือกค่าก่อนเริ่มเกม ทั้งหมดอยู่ใน Scene เดียวกัน
/// ขั้นตอน: เลือกช่วงอายุ -> เลือกโหมด -> เลือกเดี่ยว/เพื่อน -> เลือกเมนู -> โหลด Scene เกมจริง (หรือเปิด panel ห้องถ้าเลือกเพื่อน)
///
/// *** อัปเดต: เพิ่มหมวดที่ 4 = เลือกเมนู (หลังเดี่ยว/เพื่อน) ***
/// - เดี่ยว: เลือกเดี่ยว -> เลือกเมนู -> โหลด Scene ทันที
/// - เพื่อน: เลือกเพื่อน -> เลือกเมนู -> เปิด panel สร้าง/เข้าร่วมห้อง
///
/// การตั้งค่าเพิ่มเติมใน Inspector:
/// - ลาก Category_Menu (SetupCategory) ใส่ช่อง "Category Menu" (แต่ละตัวเลือกย่อย = 1 เมนู เรียง index 0,1,2,...)
/// - กรอกชื่อเมนูใน "Menu Names" ให้ index ตรงกับตัวเลือกใน Category_Menu (ใช้เก็บลง PlayerPrefs)
/// </summary>
public class GameSetupManager : MonoBehaviour
{
    public static GameSetupManager Instance { get; private set; }

    // ---------- Enum สำหรับค่าที่เลือก ----------
    public enum AgeGroup { None = -1, Child = 0, Adult = 1, Elderly = 2 }
    public enum ModeOption { None = -1, OptionA = 0, OptionB = 1 }
    public enum PlayType { None = -1, Solo = 0, Friend = 1 }

    // ---------- ค่าที่ระบบจะ "จำ" ไว้ ----------
    public AgeGroup SelectedAge { get; private set; } = AgeGroup.None;
    public ModeOption SelectedMode { get; private set; } = ModeOption.None;
    public PlayType SelectedPlayType { get; private set; } = PlayType.None;
    public int SelectedMenuIndex { get; private set; } = -1; // เมนูที่เลือก (-1 = ยังไม่เลือก)

    // ---------- ขั้นตอนปัจจุบัน ----------
    private enum SetupStep
    {
        Age = 0,
        Mode = 1,
        PlayType = 2,
        Menu = 3        // *** หมวดใหม่ ***
    }

    private SetupStep currentStep = SetupStep.Age;

    [Header("หมวดหมู่ (ลากของจริงใน Scene มาใส่ ตามลำดับ)")]
    [SerializeField] private SetupCategory categoryAge;
    [SerializeField] private SetupCategory categoryMode;
    [SerializeField] private SetupCategory categoryPlayType;
    [SerializeField] private SetupCategory categoryMenu;   // *** หมวดเมนูใหม่ ***

    [Header("รายชื่อเมนู (index ให้ตรงกับตัวเลือกใน Category Menu)")]
    [SerializeField] private List<string> menuNames = new List<string>();

    [Header("Panel ย่อยสำหรับโหมดเพื่อน (สร้างห้อง/เข้าร่วมห้อง)")]
    [SerializeField] private GameObject friendRoomPanel;

    [Header("Scene ปลายทาง (แยกตามคู่ Mode x PlayType ทั้ง 4 แบบ)")]
    [SerializeField] private string sceneNormalSolo;
    [SerializeField] private string sceneNormalFriend;
    [SerializeField] private string sceneFireSolo;
    [SerializeField] private string sceneFireFriend;

    [Header("PlayerPrefs Keys (ไม่ต้องแก้ถ้าไม่จำเป็น)")]
    [SerializeField] private string prefKeyAge = "Setup_AgeGroup";
    [SerializeField] private string prefKeyMode = "Setup_Mode";
    [SerializeField] private string prefKeyPlayType = "Setup_PlayType";
    [SerializeField] private string prefKeyMenu = "Setup_MenuIndex"; // *** key ใหม่ ***

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentStep = SetupStep.Age;
        if (friendRoomPanel != null) friendRoomPanel.SetActive(false);
        ShowCurrentStep();
    }

    // =====================================================
    //  เรียกจาก SetupCategory ตอนผู้เล่นกดซ้ำที่ตัวเลือกเดิม (ยืนยันแล้ว)
    // =====================================================
    public void OnConfirmSelection(int selectedIndex)
    {
        switch (currentStep)
        {
            case SetupStep.Age:
                SelectedAge = (AgeGroup)selectedIndex;
                Debug.Log($"[GameSetupManager] ยืนยันช่วงอายุ: {SelectedAge}");
                GoToNextStep();
                break;

            case SetupStep.Mode:
                SelectedMode = (ModeOption)selectedIndex;
                Debug.Log($"[GameSetupManager] ยืนยันโหมด: {SelectedMode}");
                GoToNextStep();
                break;

            case SetupStep.PlayType:
                SelectedPlayType = (PlayType)selectedIndex;
                Debug.Log($"[GameSetupManager] ยืนยันรูปแบบการเล่น: {SelectedPlayType}");
                GoToNextStep(); // *** ไปหมวดเมนูต่อ (ไม่โหลดซีนทันที) ***
                break;

            case SetupStep.Menu:
                SelectedMenuIndex = selectedIndex;
                string menuName = GetMenuName(selectedIndex);
                Debug.Log($"[GameSetupManager] ยืนยันเมนู: [{selectedIndex}] {menuName}");
                HandleFinalConfirm(); // *** ทำขั้นสุดท้ายหลังเลือกเมนู ***
                break;
        }
    }

    // =====================================================
    //  จัดการผลลัพธ์หลังเลือกเมนู (ขั้นสุดท้าย)
    // =====================================================
    private void HandleFinalConfirm()
    {
        SaveToPlayerPrefs();

        if (SelectedPlayType == PlayType.Solo)
        {
            LoadGameScene(); // เดี่ยว -> โหลดซีนเกมจริงทันที
        }
        else if (SelectedPlayType == PlayType.Friend)
        {
            ShowFriendRoomPanel(); // เพื่อน -> เปิด panel ห้อง
        }
    }

    private void ShowFriendRoomPanel()
    {
        // ปิดหมวดเมนู (หมวดที่กำลังแสดงอยู่) แล้วเปิด panel ห้องแทน
        if (categoryMenu != null) categoryMenu.SetCategoryActive(false);

        if (friendRoomPanel != null) friendRoomPanel.SetActive(true);
        else Debug.LogWarning("[GameSetupManager] ยังไม่ได้ลาก Friend Room Panel ใส่ใน Inspector");
    }

    public void OnCreateRoomPressed()
    {
        string targetScene = GetTargetSceneName();
        Debug.Log($"[GameSetupManager] กดสร้างห้อง -> Scene ที่ควรไปในอนาคต: {targetScene}");
        // TODO: เชื่อมต่อระบบสร้างห้อง multiplayer แล้วค่อยเรียก SceneManager.LoadScene(targetScene)
    }

    public void OnJoinRoomPressed()
    {
        string targetScene = GetTargetSceneName();
        Debug.Log($"[GameSetupManager] กดเข้าร่วมห้อง -> Scene ที่ควรไปในอนาคต: {targetScene}");
        // TODO: เชื่อมต่อระบบเข้าร่วมห้อง multiplayer แล้วค่อยเรียก SceneManager.LoadScene(targetScene)
    }

    // =====================================================
    //  ปุ่ม "ย้อนกลับ"
    // =====================================================
    public void OnBackButtonPressed()
    {
        // ถ้าอยู่ที่ Friend Room Panel -> ย้อนกลับไปหมวดเมนู
        if (friendRoomPanel != null && friendRoomPanel.activeSelf)
        {
            friendRoomPanel.SetActive(false);
            if (categoryMenu != null) categoryMenu.SetCategoryActive(true);
            return;
        }

        if (currentStep == SetupStep.Age)
        {
            Debug.Log("[GameSetupManager] อยู่ขั้นตอนแรกแล้ว ไม่สามารถย้อนกลับได้");
            return;
        }

        currentStep--;
        ShowCurrentStep();
    }

    // =====================================================
    //  ฟังก์ชันภายใน
    // =====================================================
    private void GoToNextStep()
    {
        if (currentStep == SetupStep.Menu) return; // หมวดสุดท้ายแล้ว
        currentStep++;
        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        if (categoryAge != null)      categoryAge.SetCategoryActive(currentStep == SetupStep.Age);
        if (categoryMode != null)     categoryMode.SetCategoryActive(currentStep == SetupStep.Mode);
        if (categoryPlayType != null) categoryPlayType.SetCategoryActive(currentStep == SetupStep.PlayType);
        if (categoryMenu != null)     categoryMenu.SetCategoryActive(currentStep == SetupStep.Menu);
    }

    private void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetInt(prefKeyAge, (int)SelectedAge);
        PlayerPrefs.SetInt(prefKeyMode, (int)SelectedMode);
        PlayerPrefs.SetInt(prefKeyPlayType, (int)SelectedPlayType);
        PlayerPrefs.SetInt(prefKeyMenu, SelectedMenuIndex);
        PlayerPrefs.Save();

        Debug.Log($"[GameSetupManager] บันทึกแล้ว -> Age: {SelectedAge}, Mode: {SelectedMode}, PlayType: {SelectedPlayType}, Menu: [{SelectedMenuIndex}] {GetMenuName(SelectedMenuIndex)}");
    }

    private string GetMenuName(int index)
    {
        if (index >= 0 && index < menuNames.Count) return menuNames[index];
        return "(ไม่มีชื่อ)";
    }

    private void LoadGameScene()
    {
        string targetScene = GetTargetSceneName();
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError($"[GameSetupManager] ยังไม่ได้ตั้งชื่อ Scene สำหรับคู่ Mode: {SelectedMode}, PlayType: {SelectedPlayType}");
            return;
        }
        Debug.Log($"[GameSetupManager] โหลด Scene: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }

    private string GetTargetSceneName()
    {
        if (SelectedMode == ModeOption.OptionA && SelectedPlayType == PlayType.Solo)   return sceneNormalSolo;
        if (SelectedMode == ModeOption.OptionA && SelectedPlayType == PlayType.Friend) return sceneNormalFriend;
        if (SelectedMode == ModeOption.OptionB && SelectedPlayType == PlayType.Solo)   return sceneFireSolo;
        if (SelectedMode == ModeOption.OptionB && SelectedPlayType == PlayType.Friend) return sceneFireFriend;

        Debug.LogError("[GameSetupManager] ไม่พบ Mode หรือ PlayType ที่เลือก (None)");
        return null;
    }
}