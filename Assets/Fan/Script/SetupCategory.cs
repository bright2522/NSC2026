using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// แทนหมวดหมู่หนึ่งหมวด (เช่น หมวด "ช่วงอายุ" ที่มี 3 ตัวเลือก, หมวด "โหมด 1" ที่มี 2 ตัวเลือก)
/// เก็บ list ของ SelectableOption และจัดการตรรกะ "กดเลือก / กดซ้ำเพื่อยืนยัน"
///
/// ตรรกะการทำงาน:
/// - กดตัวเลือกที่ "ยังไม่ active" -> ตัวนั้นขยายขึ้นมา (ตัวอื่นที่ active อยู่ก่อนจะหดกลับ)
/// - กดตัวเลือกที่ "active อยู่แล้ว" (กดซ้ำ) -> ถือเป็นการยืนยัน -> แจ้งไปที่ GameSetupManager ให้ไปหมวดถัดไป
///
/// วิธีใช้:
/// - สร้าง GameObject ว่างๆ ชื่อ เช่น "Category_Age" แนบสคริปต์นี้
/// - ลาก GameObject ตัวเลือกทั้งหมดในหมวดนี้ (ที่มี SelectableOption ติดอยู่) ใส่ใน list "Options"
/// - ลาก Panel หลักของหมวดนี้ (panel ที่ครอบ UI ทั้งหมวด เช่น "Panel_Age") ใส่ใน "Category Root Panel"
///   (panel นี้ใช้เปิด/ปิดตอนสลับไปหมวดอื่น แยกจาก background panel ของตัวเลือกย่อย)
/// </summary>
public class SetupCategory : MonoBehaviour
{
    [Header("Panel หลักของหมวดนี้ (ครอบทั้งหมวด)")]
    [Tooltip("Panel ที่ครอบ UI ทั้งหมวดหมู่นี้ จะเปิดตอนถึงตาหมวดนี้ และปิดตอนสลับไปหมวดอื่น")]
    [SerializeField] private GameObject categoryRootPanel;

    [Header("รายการตัวเลือกในหมวดนี้")]
    [SerializeField] private List<SelectableOption> options = new List<SelectableOption>();

    // index ของตัวเลือกที่ "ถูกเลือกไว้ก่อน" (ขยายอยู่) ยังไม่ confirm
    // -1 = ยังไม่มีตัวไหนถูกเลือก
    private int currentIndex = -1;

    /// <summary>
    /// ค่า index ของตัวเลือกที่ถูกเลือก/ยืนยันอยู่ในหมวดนี้
    /// </summary>
    public int CurrentIndex => currentIndex;

    public int OptionCount => options.Count;

    private void Awake()
    {
        // ผูกตัวเลือกแต่ละอันให้รู้ว่าตัวเองสังกัดหมวดนี้ (ใช้ตอนแจ้งกลับเมื่อถูกคลิก)
        foreach (var option in options)
        {
            if (option != null)
            {
                option.SetParentCategory(this);
            }
        }
    }

    /// <summary>
    /// เปิด/ปิด panel หลักของหมวดนี้ (เรียกจาก GameSetupManager ตอนสลับหมวด)
    /// </summary>
    public void SetCategoryActive(bool isActive)
    {
        if (categoryRootPanel != null)
        {
            categoryRootPanel.SetActive(isActive);
        }

        if (isActive)
        {
            // ทุกครั้งที่เข้าหมวดนี้ (รวมถึงตอนย้อนกลับมา) ให้เริ่มจากสถานะยังไม่มีตัวไหนถูกเลือก
            ResetSelection();
        }
    }

    /// <summary>
    /// รีเซ็ตหมวดนี้ให้ไม่มีตัวเลือกไหนถูกเลือกอยู่ (ทุกตัวหดกลับขนาดปกติ)
    /// ใช้ตอนกลับเข้าหมวดนี้ใหม่ หรือตอนย้อนกลับมาจากหมวดถัดไป
    /// </summary>
    public void ResetSelection()
    {
        currentIndex = -1;

        foreach (var option in options)
        {
            if (option != null)
            {
                option.SetActiveOption(false);
            }
        }
    }

    /// <summary>
    /// เรียกจาก SelectableOption ตอนถูกคลิก/แตะ
    /// </summary>
    public void OnOptionClicked(SelectableOption clickedOption)
    {
        int clickedIndex = options.IndexOf(clickedOption);
        if (clickedIndex == -1) return; // ตัวเลือกนี้ไม่ได้อยู่ใน list ของหมวดนี้ (ไม่ควรเกิดขึ้น)

        if (clickedIndex == currentIndex)
        {
            // กดซ้ำตัวที่ขยายอยู่แล้ว -> ยืนยันตัวเลือกนี้ -> แจ้งไปที่ GameSetupManager
            if (GameSetupManager.Instance != null)
            {
                GameSetupManager.Instance.OnConfirmSelection(currentIndex);
            }
        }
        else
        {
            // กดตัวใหม่ -> สลับ highlight ไปตัวที่กด (ตัวเดิมหดกลับอัตโนมัติ)
            currentIndex = clickedIndex;
            RefreshHighlight();
        }
    }

    /// <summary>
    /// อัปเดต highlight (scale ขยาย/หด) ของทุกตัวเลือกในหมวดนี้ ให้ตรงกับ currentIndex
    /// </summary>
    private void RefreshHighlight()
    {
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] != null)
            {
                options[i].SetActiveOption(i == currentIndex);
            }
        }
    }
}
