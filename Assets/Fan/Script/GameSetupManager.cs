using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// คุม flow การเลือกค่าก่อนเริ่มเกม (อยู่ Scene เดียว)
/// อายุ -> โหมด -> [เด้งไป UI ของโหมดนั้น] ...
/// *** อัปเดต: ยืนยันโหมดแล้วเด้งไป UI ที่กำหนดไว้ต่อโหมด (Mode Panels) ***
/// </summary>
public class GameSetupManager : MonoBehaviour
{
    public static GameSetupManager Instance { get; private set; }

    public enum AgeGroup { None = -1, Child = 0, Adult = 1, Elderly = 2 }
    public enum ModeOption { None = -1, OptionA = 0, OptionB = 1 }
    public enum PlayType { None = -1, Solo = 0, Friend = 1 }

    public AgeGroup SelectedAge { get; private set; } = AgeGroup.None;
    public ModeOption SelectedMode { get; private set; } = ModeOption.None;
    public PlayType SelectedPlayType { get; private set; } = PlayType.None;
    public int SelectedMenuIndex { get; private set; } = -1;

    private enum SetupStep { Age = 0, Mode = 1, PlayType = 2, Menu = 3 }
    private SetupStep currentStep = SetupStep.Age;

    [Header("หมวดหมู่ (ลากของจริงใน Scene มาใส่ ตามลำดับ)")]
    [SerializeField] private SetupCategory categoryAge;
    [SerializeField] private SetupCategory categoryMode;
    [SerializeField] private SetupCategory categoryPlayType;
    [SerializeField] private SetupCategory categoryMenu;

    [Header("*** UI ที่จะเด้งไปตามโหมด (ลากใส่ตรง ๆ ไม่ต้องนับเลข) ***")]
    [Tooltip("เลือกโหมดแรก (OptionA) แล้วเด้งไปหน้านี้")]
    [SerializeField] private GameObject panelModeA;
    [Tooltip("เลือกโหมดสอง (OptionB) แล้วเด้งไปหน้านี้")]
    [SerializeField] private GameObject panelModeB;

    [Header("รายชื่อเมนู (index ให้ตรงกับตัวเลือกใน Category Menu)")]
    [SerializeField] private List<string> menuNames = new List<string>();

    [Header("Panel ย่อยสำหรับโหมดเพื่อน (สร้างห้อง/เข้าร่วมห้อง)")]
    [SerializeField] private GameObject friendRoomPanel;

    [Header("Scene ปลายทาง (แยกตามคู่ Mode x PlayType ทั้ง 4 แบบ)")]
    [SerializeField] private string sceneNormalSolo;
    [SerializeField] private string sceneNormalFriend;
    [SerializeField] private string sceneFireSolo;
    [SerializeField] private string sceneFireFriend;

    [Header("PlayerPrefs Keys")]
    [SerializeField] private string prefKeyAge = "Setup_AgeGroup";
    [SerializeField] private string prefKeyMode = "Setup_Mode";
    [SerializeField] private string prefKeyPlayType = "Setup_PlayType";
    [SerializeField] private string prefKeyMenu = "Setup_MenuIndex";

    private int activeModePanel = -1; // -1 = ไม่ได้เปิด mode panel อยู่

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        currentStep = SetupStep.Age;
        if (friendRoomPanel != null) friendRoomPanel.SetActive(false);
        HideAllModePanels();
        ShowCurrentStep();
    }

    public void OnConfirmSelection(int selectedIndex)
    {
        switch (currentStep)
        {
            case SetupStep.Age:
                SelectedAge = (AgeGroup)selectedIndex;
                Debug.Log($"[Setup] ยืนยันช่วงอายุ: {SelectedAge}");
                GoToNextStep();
                break;

            case SetupStep.Mode:
                SelectedMode = (ModeOption)selectedIndex;
                Debug.Log($"[Setup] ยืนยันโหมด: {SelectedMode}");
                HandleModeConfirmed(selectedIndex); // *** เด้งไป UI ของโหมดนั้น ***
                break;

            case SetupStep.PlayType:
                SelectedPlayType = (PlayType)selectedIndex;
                Debug.Log($"[Setup] ยืนยันรูปแบบการเล่น: {SelectedPlayType}");
                GoToNextStep();
                break;

            case SetupStep.Menu:
                SelectedMenuIndex = selectedIndex;
                Debug.Log($"[Setup] ยืนยันเมนู: [{selectedIndex}] {GetMenuName(selectedIndex)}");
                HandleFinalConfirm();
                break;
        }
    }

    // *** ยืนยันโหมด -> เด้งไปหน้าตามโหมดตรง ๆ ***
    private void HandleModeConfirmed(int modeIndex)
    {
        // เลือกหน้าตามโหมด
        GameObject targetPanel = null;
        if (modeIndex == (int)ModeOption.OptionA) targetPanel = panelModeA;
        else if (modeIndex == (int)ModeOption.OptionB) targetPanel = panelModeB;

        if (targetPanel != null)
        {
            if (categoryMode != null) categoryMode.SetCategoryActive(false);
            HideAllModePanels();
            targetPanel.SetActive(true);
            activeModePanel = modeIndex;
        }
        else
        {
            // ไม่ได้ลากหน้าใส่ -> ใช้ flow เดิม (ไปหมวด PlayType)
            GoToNextStep();
        }
    }

    private void HideAllModePanels()
    {
        if (panelModeA != null) panelModeA.SetActive(false);
        if (panelModeB != null) panelModeB.SetActive(false);
        activeModePanel = -1;
    }

    private void HandleFinalConfirm()
    {
        SaveToPlayerPrefs();
        if (SelectedPlayType == PlayType.Solo) LoadGameScene();
        else if (SelectedPlayType == PlayType.Friend) ShowFriendRoomPanel();
    }

    private void ShowFriendRoomPanel()
    {
        if (categoryMenu != null) categoryMenu.SetCategoryActive(false);
        if (friendRoomPanel != null) friendRoomPanel.SetActive(true);
        else Debug.LogWarning("[Setup] ยังไม่ได้ลาก Friend Room Panel");
    }

    public void OnCreateRoomPressed()
    {
        Debug.Log($"[Setup] กดสร้างห้อง -> Scene: {GetTargetSceneName()}");
    }

    public void OnJoinRoomPressed()
    {
        Debug.Log($"[Setup] กดเข้าร่วมห้อง -> Scene: {GetTargetSceneName()}");
    }

    public void OnBackButtonPressed()
    {
        // ถ้ากำลังเปิด mode panel อยู่ -> ย้อนกลับไปหมวดโหมด
        if (activeModePanel != -1)
        {
            HideAllModePanels();
            if (categoryMode != null) categoryMode.SetCategoryActive(true);
            currentStep = SetupStep.Mode;
            return;
        }

        if (friendRoomPanel != null && friendRoomPanel.activeSelf)
        {
            friendRoomPanel.SetActive(false);
            if (categoryMenu != null) categoryMenu.SetCategoryActive(true);
            return;
        }

        if (currentStep == SetupStep.Age)
        {
            Debug.Log("[Setup] อยู่ขั้นแรกแล้ว ย้อนกลับไม่ได้");
            return;
        }

        currentStep--;
        ShowCurrentStep();
    }

    private void GoToNextStep()
    {
        if (currentStep == SetupStep.Menu) return;
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
    }

    private string GetMenuName(int index)
        => (index >= 0 && index < menuNames.Count) ? menuNames[index] : "(ไม่มีชื่อ)";

    private void LoadGameScene()
    {
        string targetScene = GetTargetSceneName();
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError($"[Setup] ยังไม่ได้ตั้งชื่อ Scene สำหรับ {SelectedMode} x {SelectedPlayType}");
            return;
        }
        SceneManager.LoadScene(targetScene);
    }

    private string GetTargetSceneName()
    {
        if (SelectedMode == ModeOption.OptionA && SelectedPlayType == PlayType.Solo)   return sceneNormalSolo;
        if (SelectedMode == ModeOption.OptionA && SelectedPlayType == PlayType.Friend) return sceneNormalFriend;
        if (SelectedMode == ModeOption.OptionB && SelectedPlayType == PlayType.Solo)   return sceneFireSolo;
        if (SelectedMode == ModeOption.OptionB && SelectedPlayType == PlayType.Friend) return sceneFireFriend;
        return null;
    }
}