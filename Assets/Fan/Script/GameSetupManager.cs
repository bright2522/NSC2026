using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// คุม flow การเลือกค่าก่อนเริ่มเกม ทั้งหมดอยู่ใน Scene เดียวกัน
/// ขั้นตอน: เลือกช่วงอายุ -> เลือกโหมด -> เลือกเดี่ยว/เพื่อน -> โหลด Scene เกมจริง (หรือเปิด panel ห้องถ้าเลือกเพื่อน)
///
/// การควบคุม (ทั้งหมดผ่านการกด/แตะบนจอ):
/// - กดที่ตัวเลือก          : ตัวเลือกนั้นขยายขึ้นมา (เลือกไว้ก่อน ยังไม่ยืนยัน)
/// - กดซ้ำที่ตัวเดียวกัน      : ยืนยันตัวเลือกนั้น แล้วไปหมวดถัดไป
/// - ปุ่ม "ย้อนกลับ" บนจอ     : ย้อนไปหมวดก่อนหน้า (หมวดที่ย้อนไปจะรีเซ็ต ไม่มีตัวเลือกไหนถูกเลือกไว้)
///
/// หมวดที่ 3 (เดี่ยว/เพื่อน) เป็นหมวดพิเศษ:
/// - ถ้ายืนยัน "เดี่ยว" (index 0)  -> บันทึกค่าทั้งหมด + โหลด Scene เกมจริงทันที
/// - ถ้ายืนยัน "เพื่อน" (index 1) -> ไม่โหลด Scene ทันที แต่เปิด Panel ย่อย (สร้างห้อง/เข้าร่วมห้อง) ต่อ
///   ปุ่ม "สร้างห้อง" / "เข้าร่วมห้อง" ใน panel ย่อยนั้น ผูกกับฟังก์ชัน OnCreateRoomPressed() / OnJoinRoomPressed()
///   (ปัจจุบันยังไม่ทำอะไรต่อ เป็นจุดที่เตรียมไว้สำหรับต่อระบบ multiplayer ในอนาคต)
///
/// วิธีใช้งานคร่าวๆ:
/// 1. สร้าง Empty GameObject ชื่อ "GameSetupManager" แล้วแนบสคริปต์นี้
/// 2. สร้างหมวดหมู่ 3 อัน (Category_Age, Category_Mode, Category_PlayType) แต่ละอันแนบสคริปต์ SetupCategory
///    และแต่ละหมวดมีตัวเลือกย่อยที่แนบสคริปต์ SelectableOption
///    - Category_PlayType ต้องมีตัวเลือก index 0 = "เดี่ยว", index 1 = "เพื่อน" (เรียงตามนี้ใน list ห้ามสลับ)
/// 3. ลาก SetupCategory ทั้ง 3 อัน ใส่ใน Inspector ของ GameSetupManager ตามลำดับ (Age, Mode, PlayType)
/// 4. ลาก Panel ย่อย "ห้อง" (มีปุ่มสร้างห้อง/เข้าร่วมห้อง) ใส่ในช่อง "Friend Room Panel"
/// 5. สร้างปุ่ม UI "ย้อนกลับ" แล้วผูก OnClick() ให้เรียก GameSetupManager.OnBackButtonPressed()
/// 6. ปุ่ม "สร้างห้อง" ผูก OnClick() เรียก OnCreateRoomPressed(), ปุ่ม "เข้าร่วมห้อง" ผูก OnJoinRoomPressed()
/// 7. พิมพ์ชื่อ Scene เกมจริงลงในช่อง "Game Scene Name" ใน Inspector
/// </summary>
public class GameSetupManager : MonoBehaviour
{
    public static GameSetupManager Instance { get; private set; }

    // ---------- Enum สำหรับค่าที่เลือก ----------
    public enum AgeGroup
    {
        None = -1,
        Child = 0,      // เด็ก
        Adult = 1,      // ผู้ใหญ่
        Elderly = 2     // ผู้สูงอายุ
    }

    public enum ModeOption
    {
        None = -1,
        OptionA = 0,
        OptionB = 1
    }

    public enum PlayType
    {
        None = -1,
        Solo = 0,       // เดี่ยว
        Friend = 1      // เพื่อน
    }

    // ---------- ค่าที่ระบบจะ "จำ" ไว้ ----------
    public AgeGroup SelectedAge { get; private set; } = AgeGroup.None;
    public ModeOption SelectedMode { get; private set; } = ModeOption.None;
    public PlayType SelectedPlayType { get; private set; } = PlayType.None;

    // ---------- ขั้นตอนปัจจุบัน ----------
    private enum SetupStep
    {
        Age = 0,
        Mode = 1,
        PlayType = 2
    }

    private SetupStep currentStep = SetupStep.Age;

    [Header("หมวดหมู่ (ลากของจริงใน Scene มาใส่ ตามลำดับ)")]
    [SerializeField] private SetupCategory categoryAge;
    [SerializeField] private SetupCategory categoryMode;
    [SerializeField] private SetupCategory categoryPlayType;

    [Header("Panel ย่อยสำหรับโหมดเพื่อน (สร้างห้อง/เข้าร่วมห้อง)")]
    [Tooltip("Panel ที่จะเปิดขึ้นมาเมื่อผู้เล่นยืนยันเลือก 'เพื่อน' ในหมวด PlayType")]
    [SerializeField] private GameObject friendRoomPanel;

    [Header("Scene ปลายทาง (แยกตามคู่ Mode x PlayType ทั้ง 4 แบบ)")]
    [Tooltip("โหมด Normal (OptionA) + เล่นเดี่ยว (Solo)")]
    [SerializeField] private string sceneNormalSolo;

    [Tooltip("โหมด Normal (OptionA) + เล่นกับเพื่อน (Friend) — ใช้ตอนกดสร้าง/เข้าร่วมห้องสำเร็จในอนาคต")]
    [SerializeField] private string sceneNormalFriend;

    [Tooltip("โหมด Fire (OptionB) + เล่นเดี่ยว (Solo)")]
    [SerializeField] private string sceneFireSolo;

    [Tooltip("โหมด Fire (OptionB) + เล่นกับเพื่อน (Friend) — ใช้ตอนกดสร้าง/เข้าร่วมห้องสำเร็จในอนาคต")]
    [SerializeField] private string sceneFireFriend;

    [Header("PlayerPrefs Keys (ไม่ต้องแก้ถ้าไม่จำเป็น)")]
    [SerializeField] private string prefKeyAge = "Setup_AgeGroup";
    [SerializeField] private string prefKeyMode = "Setup_Mode";
    [SerializeField] private string prefKeyPlayType = "Setup_PlayType";

    private void Awake()
    {
        // ป้องกันมี Manager ซ้ำซ้อนถ้าเผลอกดเข้าซีนนี้ใหม่
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // เริ่มต้นที่ขั้นตอนแรกเสมอ และปิด panel ห้องไว้ก่อน
        currentStep = SetupStep.Age;

        if (friendRoomPanel != null)
        {
            friendRoomPanel.SetActive(false);
        }

        ShowCurrentStep();
    }

    // =====================================================
    //  เรียกจาก SetupCategory ตอนผู้เล่นกดซ้ำที่ตัวเลือกเดิม (ยืนยันแล้ว)
    //  selectedIndex = index ของตัวเลือกที่ถูกยืนยันในหมวดปัจจุบัน
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
                HandlePlayTypeConfirmed();
                break;
        }
    }

    // =====================================================
    //  จัดการผลลัพธ์หลังยืนยันหมวด PlayType (หมวดสุดท้าย)
    // =====================================================
    private void HandlePlayTypeConfirmed()
    {
        SaveToPlayerPrefs();

        if (SelectedPlayType == PlayType.Solo)
        {
            // เล่นคนเดียว -> โหลด Scene เกมจริงทันที (ตามคู่ Mode + Solo)
            LoadGameScene();
        }
        else if (SelectedPlayType == PlayType.Friend)
        {
            // เล่นกับเพื่อน -> ยังไม่โหลด Scene เปิด panel ย่อยให้เลือกสร้าง/เข้าร่วมห้องต่อ
            // Scene ปลายทาง (ตามคู่ Mode + Friend) จะถูกใช้ตอนกดสร้าง/เข้าร่วมห้องสำเร็จในอนาคต
            ShowFriendRoomPanel();
        }
    }

    private void ShowFriendRoomPanel()
    {
        // ปิดหมวด PlayType ลงก่อน (ซ่อนตัวเลือกเดี่ยว/เพื่อน) แล้วเปิด panel ห้องแทน
        if (categoryPlayType != null)
        {
            categoryPlayType.SetCategoryActive(false);
        }

        if (friendRoomPanel != null)
        {
            friendRoomPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[GameSetupManager] ยังไม่ได้ลาก Friend Room Panel ใส่ใน Inspector");
        }
    }

    // =====================================================
    //  ปุ่ม "สร้างห้อง" ใน Friend Room Panel (ผูก OnClick() ใน Inspector)
    //  ปัจจุบันยังไม่ implement ระบบจริง เป็นจุดเตรียมไว้สำหรับต่อ multiplayer
    //  ในอนาคต ตอนสร้างห้องสำเร็จ ให้เรียก SceneManager.LoadScene(GetTargetSceneName()) ต่อจากนี้
    // =====================================================
    public void OnCreateRoomPressed()
    {
        string targetScene = GetTargetSceneName();
        Debug.Log($"[GameSetupManager] กดสร้างห้อง (ยังไม่ implement ระบบห้องจริง) -> Scene ที่ควรไปในอนาคต: {targetScene}");
        // TODO: เชื่อมต่อระบบสร้างห้อง multiplayer ในอนาคต แล้วค่อยเรียก SceneManager.LoadScene(targetScene)
    }

    // =====================================================
    //  ปุ่ม "เข้าร่วมห้อง" ใน Friend Room Panel (ผูก OnClick() ใน Inspector)
    //  ปัจจุบันยังไม่ implement ระบบจริง เป็นจุดเตรียมไว้สำหรับต่อ multiplayer
    //  ในอนาคต ตอนเข้าร่วมห้องสำเร็จ ให้เรียก SceneManager.LoadScene(GetTargetSceneName()) ต่อจากนี้
    // =====================================================
    public void OnJoinRoomPressed()
    {
        string targetScene = GetTargetSceneName();
        Debug.Log($"[GameSetupManager] กดเข้าร่วมห้อง (ยังไม่ implement ระบบห้องจริง) -> Scene ที่ควรไปในอนาคต: {targetScene}");
        // TODO: เชื่อมต่อระบบเข้าร่วมห้อง multiplayer ในอนาคต แล้วค่อยเรียก SceneManager.LoadScene(targetScene)
    }

    // =====================================================
    //  เรียกจากปุ่ม UI "ย้อนกลับ" บนจอ (ผูกผ่าน OnClick() ใน Inspector)
    // =====================================================
    public void OnBackButtonPressed()
    {
        // ถ้าตอนนี้กำลังอยู่ที่ Friend Room Panel ให้ย้อนกลับไปหมวด PlayType ก่อน
        if (friendRoomPanel != null && friendRoomPanel.activeSelf)
        {
            friendRoomPanel.SetActive(false);

            if (categoryPlayType != null)
            {
                categoryPlayType.SetCategoryActive(true);
            }
            return;
        }

        if (currentStep == SetupStep.Age)
        {
            // อยู่ขั้นแรกแล้ว ไม่มีที่ให้ย้อนกลับไปอีก
            Debug.Log("[GameSetupManager] อยู่ขั้นตอนแรกแล้ว ไม่สามารถย้อนกลับได้");
            return;
        }

        currentStep--;
        ShowCurrentStep();
    }

    // =====================================================
    //  ฟังก์ชันภายใน: จัดการสลับหมวด และเดินหน้า step
    // =====================================================
    private void GoToNextStep()
    {
        if (currentStep == SetupStep.PlayType)
        {
            // เป็นขั้นสุดท้ายอยู่แล้ว (ฟังก์ชัน HandlePlayTypeConfirmed จัดการต่อเอง)
            return;
        }

        currentStep++;
        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        // เปิดเฉพาะหมวดของ step ปัจจุบัน ปิดหมวดอื่นทั้งหมด
        // SetCategoryActive(true) จะรีเซ็ตให้หมวดนั้นไม่มีตัวเลือกไหนถูกเลือกไว้โดยอัตโนมัติ
        if (categoryAge != null) categoryAge.SetCategoryActive(currentStep == SetupStep.Age);
        if (categoryMode != null) categoryMode.SetCategoryActive(currentStep == SetupStep.Mode);
        if (categoryPlayType != null) categoryPlayType.SetCategoryActive(currentStep == SetupStep.PlayType);
    }

    // =====================================================
    //  บันทึกค่าลง PlayerPrefs เพื่อให้ Scene เกมจริงดึงไปใช้ได้
    //  (อ่านค่ากลับด้วย PlayerPrefs.GetInt(key) ใน Scene เกม)
    // =====================================================
    private void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetInt(prefKeyAge, (int)SelectedAge);
        PlayerPrefs.SetInt(prefKeyMode, (int)SelectedMode);
        PlayerPrefs.SetInt(prefKeyPlayType, (int)SelectedPlayType);
        PlayerPrefs.Save();

        Debug.Log($"[GameSetupManager] บันทึกค่าแล้ว -> Age: {SelectedAge}, Mode: {SelectedMode}, PlayType: {SelectedPlayType}");
    }

    // =====================================================
    //  โหลด Scene เกมจริงตามคู่ "Mode x PlayType" (ใช้ตอนเลือกเดี่ยวเท่านั้น)
    //  Normal + Solo   -> sceneNormalSolo
    //  Fire   + Solo   -> sceneFireSolo
    //  (กรณี Friend จะยังไม่ถูกเรียกจากที่นี่ รอ implement ระบบห้องในอนาคต)
    // =====================================================
    private void LoadGameScene()
    {
        string targetScene = GetTargetSceneName();

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError($"[GameSetupManager] ยังไม่ได้ตั้งชื่อ Scene สำหรับคู่ Mode: {SelectedMode}, PlayType: {SelectedPlayType} ใน Inspector!");
            return;
        }

        Debug.Log($"[GameSetupManager] Mode: {SelectedMode}, PlayType: {SelectedPlayType} -> กำลังโหลด Scene: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }

    // =====================================================
    //  หาชื่อ Scene ที่ตรงกับคู่ Mode x PlayType ปัจจุบัน
    //  ใช้ทั้งตอนโหลด Scene จริง (Solo) และเตรียมไว้ใช้ตอนกดสร้าง/เข้าร่วมห้องสำเร็จ (Friend) ในอนาคต
    // =====================================================
    private string GetTargetSceneName()
    {
        if (SelectedMode == ModeOption.OptionA && SelectedPlayType == PlayType.Solo)
        {
            return sceneNormalSolo;
        }
        else if (SelectedMode == ModeOption.OptionA && SelectedPlayType == PlayType.Friend)
        {
            return sceneNormalFriend;
        }
        else if (SelectedMode == ModeOption.OptionB && SelectedPlayType == PlayType.Solo)
        {
            return sceneFireSolo;
        }
        else if (SelectedMode == ModeOption.OptionB && SelectedPlayType == PlayType.Friend)
        {
            return sceneFireFriend;
        }

        Debug.LogError("[GameSetupManager] ไม่พบ Mode หรือ PlayType ที่เลือกไว้ (ค่าเป็น None) ไม่สามารถระบุ Scene ปลายทางได้");
        return null;
    }
}