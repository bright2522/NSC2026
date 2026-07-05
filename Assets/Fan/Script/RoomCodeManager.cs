using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoomCodeManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject roomCodePanel;        // ตัวแป้นคีย์บอร์ด + ปุ่ม X (ButtonGridContainer + CloseButton)
    public TextMeshProUGUI placeholderText; // โจทก์ใหม่: ข้อความ "Enter Room Code" (ตัวเล็ก)
    public TextMeshProUGUI codeDisplayText; // ข้อความแสดงตัวเลข 6 หลัก (ตัวใหญ่)
    public TextMeshProUGUI errorText;       // ข้อความแจ้งเตือน NotFound (สีแดง)

    [Header("Settings")]
    private string currentInput = "";       
    private int maxCodeLength = 6;          
    private string correctRoomCode = "123456"; 

    void Start()
    {
        if (roomCodePanel != null) roomCodePanel.SetActive(false);
        if (errorText != null) errorText.text = "";
        
        // ตอนเริ่มเกม ให้โชว์คำว่า Enter Room Code ค้างไว้ และเคลียร์ช่องตัวเลขเป็นว่างเปล่า
        if (placeholderText != null) placeholderText.gameObject.SetActive(true);
        if (codeDisplayText != null) codeDisplayText.text = "";
    }

    // กดปุ่มโหมดแข่งขันเพื่อเปิดแป้นพิมพ์
    public void OpenPanel()
    {
        if (roomCodePanel != null) roomCodePanel.SetActive(true);
        
        // เงื่อนไข: สั่งให้ข้อความ "Enter Room Code" หายไปทันที!
        if (placeholderText != null) placeholderText.gameObject.SetActive(false);
        
        ClearInput();
    }

    // กดปุ่ม X เพื่อปิดแป้นพิมพ์
    public void ClosePanel()
    {
        if (roomCodePanel != null) roomCodePanel.SetActive(false);
        
        currentInput = "";
        if (codeDisplayText != null) codeDisplayText.text = "";
        if (errorText != null) errorText.text = "";
        
        // คืนค่า: สั่งให้ข้อความ "Enter Room Code" กลับมาแสดงผลใหม่
        if (placeholderText != null) placeholderText.gameObject.SetActive(true);
    }

    public void PressNumber(string number)
    {
        if (errorText != null) errorText.text = "";

        if (currentInput.Length < maxCodeLength)
        {
            currentInput += number;
            UpdateDisplay();
        }
    }

    public void PressDelete()
    {
        if (errorText != null) errorText.text = "";

        if (currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        if (codeDisplayText != null)
        {
            codeDisplayText.text = currentInput;
            
            // ตัวเลือกเสริม: ถ้าลบจนหมด จะให้แสดงเป็นขีดล่าง 6 ขีดตัวใหญ่ๆ ก็ได้ครับ
            if (currentInput == "")
            {
                codeDisplayText.text = "______"; 
            }
        }
    }

    void ClearInput()
    {
        currentInput = "";
        if (errorText != null) errorText.text = "";
        if (codeDisplayText != null) codeDisplayText.text = "______"; 
    }

    public void PressConfirm()
    {
        if (currentInput.Length < maxCodeLength)
        {
            if (errorText != null) errorText.text = "NotFound";
            return;
        }

        if (currentInput == correctRoomCode)
        {
            if (errorText != null) errorText.text = "<color=green>Success!</color>";
        }
        else
        {
            if (errorText != null) errorText.text = "NotFound";
        }
    }
}