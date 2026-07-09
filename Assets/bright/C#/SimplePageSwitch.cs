using UnityEngine;

// ง่ายสุด: กดปุ่ม -> ซ่อนหน้านี้ โชว์หน้าโน้น
// ไม่มี index ไม่มีลำดับ แค่ลาก 2 หน้ามาใส่ปุ่ม
public class SimplePageSwitch : MonoBehaviour
{
    // ผูกฟังก์ชันนี้กับปุ่ม แล้วในปุ่มลาก 2 อย่างใส่:
    //   pageToHide = หน้าที่อยากซ่อน
    //   pageToShow = หน้าที่อยากโชว์
    public void Switch(GameObject pageToShow)
    {
        // ซ่อนหน้าที่ปุ่มนี้อยู่ (ตัวเอง หรือ parent panel)
        // แล้วโชว์หน้าใหม่
        pageToShow.SetActive(true);
    }

    // ซ่อนหน้า (ผูกเพิ่มอีกบรรทัดในปุ่มเดียวกัน)
    public void Hide(GameObject page)
    {
        page.SetActive(false);
    }

    // โชว์หน้า
    public void Show(GameObject page)
    {
        page.SetActive(true);
    }
}