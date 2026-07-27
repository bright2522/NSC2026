using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CountdownController : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("ใส่ UI Image ที่ต้องการใช้แสดงผล")]
    public Image countdownDisplay;

    [Header("Countdown Sprites")]
    [Tooltip("ใส่รูปภาพเรียงลำดับ: 0=เลข 3, 1=เลข 2, 2=เลข 1, 3=Ready")]
    public Sprite[] countdownSprites;

    [Header("Settings")]
    [Tooltip("ระยะเวลาการแสดงผลของแต่ละรูป (วินาที)")]
    public float delayBetweenSprites = 1f;

    [Tooltip("ระยะเวลาในการเล่นเอฟเฟกต์ Pop-in (วินาที)")]
    public float popInDuration = 0.3f;

    // เก็บ RectTransform และขนาดเดิมที่ตั้งไว้ใน Inspector
    private RectTransform rectTransform;
    private Vector3 originalScale;

    void Awake()
    {
        if (countdownDisplay != null)
        {
            rectTransform = countdownDisplay.GetComponent<RectTransform>();
            // ดึงค่า Scale ปัจจุบันที่คุณปรับไว้ใน Inspector เก็บไว้เป็นขนาดเป้าหมาย
            originalScale = rectTransform.localScale;
        }
    }

    void Start()
    {
        // เริ่มทำงานนับถอยหลังทันทีเมื่อเริ่มเกม
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        if (countdownDisplay == null || countdownSprites.Length == 0)
        {
            Debug.LogWarning("กรุณาใส่ Countdown Display หรือ Countdown Sprites ใน Inspector ให้ครบถ้วน!");
            yield break;
        }

        // เริ่มต้นด้วยการซ่อนรูปภาพ
        countdownDisplay.gameObject.SetActive(false);

        // แสดงรูปภาพทีละรูปตามลำดับใน Array
        for (int i = 0; i < countdownSprites.Length; i++)
        {
            // เปลี่ยนรูปภาพ
            countdownDisplay.sprite = countdownSprites[i];
            
            // เปิดแสดงรูปภาพ
            countdownDisplay.gameObject.SetActive(true);

            // เริ่มทำเอฟเฟกต์ Pop-in โดยขยายจาก 0 ไปยัง originalScale ที่ตั้งไว้ใน Inspector
            LeanTween.scale(rectTransform, originalScale, popInDuration)
                     .setFrom(Vector3.zero)
                     .setEaseOutBack(); // เพิ่มความดึ๋งๆ นุ่มนวลตอนขยายสุด

            // รอเวลาตามที่กำหนดไว้ก่อนเปลี่ยนรูปถัดไป
            yield return new WaitForSeconds(delayBetweenSprites);

            // ปิดแสดงรูปภาพก่อนเปลี่ยนเป็นรูปถัดไป
            countdownDisplay.gameObject.SetActive(false);
        }

        // เมื่อนับถอยหลังจบ
        OnCountdownFinished();
    }

    void OnCountdownFinished()
    {
        Debug.Log("เริ่มเกมได้!");
        // ใส่โค้ดปลดล็อกตัวละคร หรือเริ่มสปอว์นศัตรูตรงนี้
    }
}