using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// คุม flow การเลือกค่าก่อนเริ่มเกม ทั้งหมดอยู่ใน Scene เดียวกัน
/// ขั้นตอน: เลือกช่วงอายุ -> เลือกโหมด 1 -> เลือกโหมด 2 -> โหลด Scene เกมจริง
/// 
/// วิธีใช้งานคร่าวๆ:
/// 1. สร้าง Empty GameObject ชื่อ "GameSetupManager" แล้วแนบสคริปต์นี้
/// 2. ลาก Panel ทั้ง 3 อัน (PanelAge, PanelMode1, PanelMode2) ใส่ใน Inspector
/// 3. ที่ปุ่มแต่ละปุ่ม ผูก OnClick() เรียกฟังก์ชัน SelectAge(0/1/2), SelectMode1(0/1), SelectMode2(0/1)
/// 4. ปุ่ม Back ผูก OnClick() เรียก GoBack()
/// 5. พิมพ์ชื่อ Scene เกมจริงลงในช่อง "Game Scene Name" ใน Inspector
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

    // ---------- ค่าที่ระบบจะ "จำ" ไว้ ----------
    public AgeGroup SelectedAge { get; private set; } = AgeGroup.None;
    public ModeOption SelectedMode1 { get; private set; } = ModeOption.None;
    public ModeOption SelectedMode2 { get; private set; } = ModeOption.None;

    // ---------- ขั้นตอนปัจจุบัน ----------
    private enum SetupStep
    {
        Age = 0,
        Mode1 = 1,
        Mode2 = 2
    }

    private SetupStep currentStep = SetupStep.Age;

    [Header("Panels (ลากของจริงใน Scene มาใส่)")]
    [SerializeField] private GameObject panelAge;
    [SerializeField] private GameObject panelMode1;
    [SerializeField] private GameObject panelMode2;

    [Header("Scene ปลายทาง")]
    [Tooltip("พิมพ์ชื่อ Scene เกมจริงที่จะโหลดหลังเลือกครบ 3 ขั้นตอน (ต้องเป็นชื่อ Scene ที่เพิ่มใน Build Settings แล้ว)")]
    [SerializeField] private string gameSceneName;

    [Header("PlayerPrefs Keys (ไม่ต้องแก้ถ้าไม่จำเป็น)")]
    [SerializeField] private string prefKeyAge = "Setup_AgeGroup";
    [SerializeField] private string prefKeyMode1 = "Setup_Mode1";
    [SerializeField] private string prefKeyMode2 = "Setup_Mode2";

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
        // เริ่มต้นที่ขั้นตอนแรกเสมอ
        currentStep = SetupStep.Age;
        ShowCurrentStep();
    }

    // =====================================================
    //  ขั้นตอน 1: เลือกช่วงอายุ
    //  เรียกจากปุ่ม: SelectAge(0) = เด็ก, SelectAge(1) = ผู้ใหญ่, SelectAge(2) = ผู้สูงอายุ
    // =====================================================
    public void SelectAge(int ageIndex)
    {
        SelectedAge = (AgeGroup)ageIndex;
        Debug.Log($"[GameSetupManager] เลือกช่วงอายุ: {SelectedAge}");

        GoToNextStep();
    }

    // =====================================================
    //  ขั้นตอน 2: เลือกโหมดที่ 1
    //  เรียกจากปุ่ม: SelectMode1(0) = ตัวเลือก A, SelectMode1(1) = ตัวเลือก B
    // =====================================================
    public void SelectMode1(int modeIndex)
    {
        SelectedMode1 = (ModeOption)modeIndex;
        Debug.Log($"[GameSetupManager] เลือกโหมดที่ 1: {SelectedMode1}");

        GoToNextStep();
    }

    // =====================================================
    //  ขั้นตอน 3: เลือกโหมดที่ 2 (ขั้นสุดท้าย)
    //  เรียกจากปุ่ม: SelectMode2(0) = ตัวเลือก A, SelectMode2(1) = ตัวเลือก B
    // =====================================================
    public void SelectMode2(int modeIndex)
    {
        SelectedMode2 = (ModeOption)modeIndex;
        Debug.Log($"[GameSetupManager] เลือกโหมดที่ 2: {SelectedMode2}");

        // เลือกครบทั้ง 3 ขั้นแล้ว -> บันทึกค่า + โหลด Scene เกมจริง
        SaveToPlayerPrefs();
        LoadGameScene();
    }

    // =====================================================
    //  ปุ่มย้อนกลับ ใช้ปุ่มเดียวได้ในทุก panel (ยกเว้น panel แรกซึ่งไม่มีให้ย้อน)
    // =====================================================
    public void GoBack()
    {
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
    //  ฟังก์ชันภายใน: จัดการสลับ panel และเดินหน้า/ถอยหลัง step
    // =====================================================
    private void GoToNextStep()
    {
        if (currentStep == SetupStep.Mode2)
        {
            // เป็นขั้นสุดท้ายอยู่แล้ว ไม่ต้องเปลี่ยน step (ฟังก์ชัน SelectMode2 จัดการโหลดซีนเอง)
            return;
        }

        currentStep++;
        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        // ซ่อนทุก panel ก่อน แล้วเปิดเฉพาะ panel ของ step ปัจจุบัน
        if (panelAge != null) panelAge.SetActive(currentStep == SetupStep.Age);
        if (panelMode1 != null) panelMode1.SetActive(currentStep == SetupStep.Mode1);
        if (panelMode2 != null) panelMode2.SetActive(currentStep == SetupStep.Mode2);
    }

    // =====================================================
    //  บันทึกค่าลง PlayerPrefs เพื่อให้ Scene เกมจริงดึงไปใช้ได้
    //  (อ่านค่ากลับด้วย PlayerPrefs.GetInt(key) ใน Scene เกม)
    // =====================================================
    private void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetInt(prefKeyAge, (int)SelectedAge);
        PlayerPrefs.SetInt(prefKeyMode1, (int)SelectedMode1);
        PlayerPrefs.SetInt(prefKeyMode2, (int)SelectedMode2);
        PlayerPrefs.Save();

        Debug.Log($"[GameSetupManager] บันทึกค่าแล้ว -> Age: {SelectedAge}, Mode1: {SelectedMode1}, Mode2: {SelectedMode2}");
    }

    // =====================================================
    //  โหลด Scene เกมจริงตามชื่อที่กำหนดใน Inspector
    // =====================================================
    private void LoadGameScene()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[GameSetupManager] ยังไม่ได้ตั้งชื่อ Game Scene ใน Inspector! กรุณาใส่ชื่อ Scene ในช่อง 'Game Scene Name'");
            return;
        }

        Debug.Log($"[GameSetupManager] กำลังโหลด Scene: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }
}