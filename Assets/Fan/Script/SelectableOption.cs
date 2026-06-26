using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// แนบสคริปต์นี้ไว้ที่ "ตัวเลือก" แต่ละอันในหมวดหมู่ (เช่น ตัวเลือก "เด็ก", "ผู้ใหญ่", "ผู้สูงอายุ")
/// ทำหน้าที่:
/// 1. รับการกด/แตะ (คลิกเมาส์ หรือแตะจอ) ผ่าน IPointerClickHandler ของ Unity UI
/// 2. ขยายขนาดตัวเอง (scale) แบบ smooth เมื่อถูกเลือกอยู่ (active)
/// 3. หดกลับขนาดปกติเมื่อไม่ได้ถูกเลือก
/// 4. เปิด/ปิด Panel background ของตัวเองตามสถานะ active
///
/// ข้อกำหนดสำคัญ: GameObject นี้ต้องมี Image (หรือ Graphic อื่นๆ) ติดอยู่ด้วย
/// เพื่อให้ระบบ EventSystem ของ Unity UI รับรู้การคลิก/แตะได้
/// และต้องมี Canvas + GraphicRaycaster ในซีน (ปกติ Unity สร้างให้อัตโนมัติเมื่อสร้าง UI)
///
/// วิธีใช้:
/// - สร้าง GameObject ตัวเลือก (เช่น Option_Child) แนบสคริปต์นี้ (ตัว GameObject ควรมี Image ด้วย)
/// - ลาก Panel ที่จะเป็น background ของตัวเลือกนี้ใส่ในช่อง "Background Panel"
/// - ปรับค่าขนาด/ความเร็วใน Inspector ได้ตามต้องการ
/// - ไม่ต้องผูก OnClick() ใดๆ ใน Inspector ระบบจะรับคลิกเองผ่าน IPointerClickHandler
/// </summary>
public class SelectableOption : MonoBehaviour, IPointerClickHandler
{
    [Header("Background Panel ของตัวเลือกนี้")]
    [Tooltip("Panel ที่จะใช้เป็น background เฉพาะของตัวเลือกนี้ จะเปิดเมื่อ active และปิดเมื่อไม่ active")]
    [SerializeField] private GameObject backgroundPanel;

    [Header("ขนาด Scale")]
    [Tooltip("ขนาดตอนไม่ได้ถูกเลือก (ปกติ)")]
    [SerializeField] private float normalScale = 1f;

    [Tooltip("ขนาดตอนถูกเลือกอยู่ (active)")]
    [SerializeField] private float selectedScale = 1.2f;

    [Header("ความเร็ว/ลักษณะการขยับ")]
    [Tooltip("ความเร็วในการ Lerp ขนาด (ค่ายิ่งมาก ยิ่งขยับเร็ว)")]
    [SerializeField] private float transitionSpeed = 8f;

    [Tooltip("Animation Curve สำหรับควบคุมลักษณะการขยาย/หด (ใช้ปรับ ease-in/out ได้ใน Inspector)")]
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // อ้างอิงถึงหมวดหมู่ที่ตัวเลือกนี้สังกัดอยู่ (SetupCategory จะตั้งค่านี้ให้ตอน Awake/Start ของหมวด)
    private SetupCategory parentCategory;

    private bool isActiveOption = false;
    private float currentT = 0f;       // ค่าระหว่าง 0-1 สำหรับอ่านจาก curve
    private float targetT = 0f;

    private void Awake()
    {
        // ตั้งขนาดเริ่มต้นเป็นขนาดปกติก่อน
        transform.localScale = Vector3.one * normalScale;

        if (backgroundPanel != null)
        {
            backgroundPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // ขยับ currentT เข้าใกล้ targetT แบบ smooth ตามเวลา
        if (!Mathf.Approximately(currentT, targetT))
        {
            currentT = Mathf.MoveTowards(currentT, targetT, transitionSpeed * Time.deltaTime);

            float curveValue = transitionCurve.Evaluate(currentT);
            float scale = Mathf.LerpUnclamped(normalScale, selectedScale, curveValue);
            transform.localScale = Vector3.one * scale;
        }
    }

    /// <summary>
    /// ตั้งค่าหมวดหมู่ที่ตัวเลือกนี้สังกัดอยู่ (เรียกจาก SetupCategory อัตโนมัติ ไม่ต้องตั้งมือ)
    /// </summary>
    public void SetParentCategory(SetupCategory category)
    {
        parentCategory = category;
    }

    /// <summary>
    /// Unity UI จะเรียกฟังก์ชันนี้ให้เองอัตโนมัติเมื่อมีการคลิก/แตะที่ตัวเลือกนี้
    /// (ไม่ต้องไปผูก OnClick() ใน Inspector)
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (parentCategory != null)
        {
            parentCategory.OnOptionClicked(this);
        }
    }

    /// <summary>
    /// เรียกจาก SetupCategory เพื่อบอกว่าตัวเลือกนี้ถูกเลือกอยู่หรือไม่
    /// </summary>
    public void SetActiveOption(bool isActive)
    {
        isActiveOption = isActive;
        targetT = isActive ? 1f : 0f;

        if (backgroundPanel != null)
        {
            backgroundPanel.SetActive(isActive);
        }
    }

    public bool IsActiveOption => isActiveOption;
}
