using UnityEngine;

public class SettingUIController : MonoBehaviour
{
    [Header("UI Elements")]
    // ช่องสำหรับลาก Panel_Setting จาก Hierarchy มาใส่
    public GameObject settingPanel;

    [Header("Animation Settings")]
    // ความเร็วของแอนิเมชัน (หน่วยเป็นวินาที)
    public float animationSpeed = 0.3f;

    void Start()
    {
        // เมื่อเริ่มเกม ให้ซ่อนหน้าต่างไว้ก่อน
        if (settingPanel != null)
        {
            settingPanel.SetActive(true);
            settingPanel.transform.localScale = Vector3.zero;
        }
    }

    // ฟังก์ชันสำหรับ "เปิด" หน้าต่าง (นำไปผูกกับปุ่มรูปฟันเฟือง)
    public void OpenSettings()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(true); // บังคับเปิดหน้าต่างเผื่อถูกปิดไว้
            // ใช้ LeanTween ขยายขนาดเป็น 1 พร้อมเอฟเฟกต์เด้งดึ๋ง
            LeanTween.scale(settingPanel, Vector3.one, animationSpeed).setEase(LeanTweenType.easeOutBack);
        }
    }

    // ฟังก์ชันสำหรับ "ปิด" หน้าต่าง (นำไปผูกกับปุ่มตกลง หรือย้อนกลับ)
    public void CloseSettings()
    {
        if (settingPanel != null)
        {
            // ใช้ LeanTween ย่อขนาดกลับเป็น 0 พร้อมเอฟเฟกต์หดตัว
            LeanTween.scale(settingPanel, Vector3.zero, animationSpeed).setEase(LeanTweenType.easeInBack);
        }
    }
}