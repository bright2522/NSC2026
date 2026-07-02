using UnityEngine;
using UnityEngine.UI;

// คุมปุ่ม "ไปต่อ" (→) ให้กดได้เฉพาะเมื่อเลือกเมนู + วัตถุดิบครบเงื่อนไข
public class NextButtonGate : MonoBehaviour
{
    [Header("เชื่อมระบบ")]
    public MultiSelectManager selectionManager; // เช็คว่าเลือกวัตถุดิบไหม
    public MenuManager menuManager;             // เช็คว่าเลือกเมนูไหม

    [Header("ปุ่มไปต่อ")]
    public Button nextButton;                   // ปุ่มลูกศรเขียว →

    [Header("เงื่อนไข")]
    public bool requireMenu = true;             // ต้องเลือกเมนู
    public bool requireIngredient = true;       // ต้องเลือกวัตถุดิบอย่างน้อย 1

    [Header("แสดงผลตอนกดไม่ได้ (จะใส่หรือไม่ก็ได้)")]
    public GameObject lockedHint;               // ข้อความ/ไอคอน "กรุณาเลือกก่อน"

    void Update()
    {
        bool ok = true;

        if (requireMenu && (menuManager == null || !menuManager.HasMenuSelected))
            ok = false;

        if (requireIngredient && (selectionManager == null || !selectionManager.HasAnySelected))
            ok = false;

        if (nextButton != null)
            nextButton.interactable = ok;

        if (lockedHint != null)
            lockedHint.SetActive(!ok); // โชว์คำเตือนตอนกดไม่ได้
    }
}