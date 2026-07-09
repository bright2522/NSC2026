using UnityEngine;
using UnityEngine.UI;

public class TimerSystem : MonoBehaviour
{
    [Header("UI Elements")]
    public Image timerFillImage; // ลาก TimerBar_Fill มาใส่ที่นี่
    public Button finishButton;  // ลาก Finish_Button มาใส่ที่นี่

    [Header("Timer Settings")]
    public float maxTime = 10f;  // เวลาทั้งหมด (วินาที)
    private float currentTime;
    private bool isTimerRunning = true;

    void Start()
    {
        currentTime = maxTime;
        
        // ผูกฟังก์ชันเข้ากับปุ่มกดเสร็จเรียบร้อย
        if (finishButton != null)
        {
            finishButton.onClick.AddListener(OnFinishButtonPressed);
        }
    }

    void Update()
    {
        if (isTimerRunning)
        {
            if (currentTime > 0)
            {
                // ลดเวลาลงเรื่อยๆ
                currentTime -= Time.deltaTime;
                
                // อัปเดตหลอด UI (ค่า Fill Amount จะอยู่ระหว่าง 0 ถึง 1)
                timerFillImage.fillAmount = currentTime / maxTime;
            }
            else
            {
                // เวลาหมด
                currentTime = 0;
                timerFillImage.fillAmount = 0;
                isTimerRunning = false;
                TimeOut();
            }
        }
    }

    // ฟังก์ชันเมื่อกดปุ่ม "เสร็จเรียบร้อย"
    void OnFinishButtonPressed()
    {
        if (!isTimerRunning) return; // ถ้าเวลาหมดหรือกดไปแล้ว ไม่ให้ทำงานซ้ำ

        isTimerRunning = false; // หยุดเวลา
        float progress = currentTime / maxTime; // ค่าจะอยู่ระหว่าง 0.0 ถึง 1.0

        // แบ่งเป็น 3 ช่วง (เนื่องจากลดจากขวาไปซ้าย ขวาสุดคือค่า progress จะสูงใกล้ 1)
        if (progress > 0.66f)
        {
            // ช่วงที่ 1: ขวาสุด (ลดยังไม่เกิน 1/3 ของหลอด)
            ShowResult("คะแนน 100 และ เงิน 20");
        }
        else if (progress > 0.33f && progress <= 0.66f)
        {
            // ช่วงที่ 2: ตรงกลาง
            ShowResult("คะแนน 80 และ เงิน 15");
        }
        else
        {
            // ช่วงที่ 3: ซ้ายสุด (เวลาใกล้หมด)
            ShowResult("คะแนน 60 และ เงิน 10");
        }
    }

    void ShowResult(string message)
    {
        Debug.Log("แสดงรูปภาพพร้อมข้อความ: " + message);
        
        // TODO: ตรงนี้เอาไว้สั่งเปิดหน้าต่างรูปภาพที่คุณเตรียมไว้
        // ตัวอย่างเช่น: resultPanel.SetActive(true); 
        // แล้วเปลี่ยน Text ในรูปภาพตามข้อความที่ส่งมา
    }

    void TimeOut()
    {
        Debug.Log("เวลาหมดแล้ว!");
        // สิ่งที่จะให้เกิดขึ้นเมื่อผู้เล่นกดไม่ทัน
    }
}