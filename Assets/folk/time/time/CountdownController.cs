using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CountdownController : MonoBehaviour
{
    // 🔒 true ตั้งแต่เริ่มนับถอยหลังจนกว่าจะนับจบ (ใช้ล็อกการเล่นแม้ไม่มีระบบ Multiplayer เช่นตอนเทสเดี่ยว)
    public static bool IsCountdownActive { get; private set; }

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
        // ⏱️ ผูกกับเวลานับถอยหลังจริงของ MultiplayerGameManager (StartCountdown)
        // แทนที่จะนับเวลาแยกของตัวเอง จะได้ตรงกับตอนที่ผู้เล่นขยับตัวได้จริง (GameStarted)
        StartCoroutine(SyncWithServerCountdown());
    }

    IEnumerator SyncWithServerCountdown()
    {
        IsCountdownActive = true;

        if (countdownDisplay == null || countdownSprites.Length == 0)
        {
            Debug.LogWarning("กรุณาใส่ Countdown Display หรือ Countdown Sprites ใน Inspector ให้ครบถ้วน!");
            IsCountdownActive = false;
            yield break;
        }

        countdownDisplay.gameObject.SetActive(false);

        // 🎬 เล่นภาพ 3, 2, 1, Ready ตามลำดับปกติเหมือนเดิมเสมอ (ไม่รอ network ตรงนี้)
        for (int i = 0; i < countdownSprites.Length; i++)
        {
            ShowSprite(i);
            yield return new WaitForSeconds(delayBetweenSprites);
            countdownDisplay.gameObject.SetActive(false);
        }

        // ⏱️ หลังนับจอจบแล้ว ถ้าเป็นเกมมัลติเพลเยอร์ ให้รอจนกว่า GameStarted จริงจากเซิร์ฟเวอร์
        // (กันเคสที่ network countdown ยังไม่จบ ทำให้ค้างที่ "Ready" รอผู้เล่นคนอื่น)
        if (MultiplayerGameManager.Instance != null)
        {
            countdownDisplay.sprite = countdownSprites[countdownSprites.Length - 1];
            countdownDisplay.gameObject.SetActive(true);

            while (MultiplayerGameManager.IsSpawnedReady && !MultiplayerGameManager.Instance.GameStarted)
                yield return null;

            countdownDisplay.gameObject.SetActive(false);
        }

        OnCountdownFinished();
    }

    void ShowSprite(int index)
    {
        countdownDisplay.sprite = countdownSprites[index];
        countdownDisplay.gameObject.SetActive(true);

        LeanTween.scale(rectTransform, originalScale, popInDuration)
                 .setFrom(Vector3.zero)
                 .setEaseOutBack();
    }

    void OnCountdownFinished()
    {
        IsCountdownActive = false;
        Debug.Log("เริ่มเกมได้!");
        // 🔓 ตัวละครถูกปลดล็อกโดย MultiplayerGameManager.GameStarted อยู่แล้ว (ดู ThirdPersonController.Update())
    }

    void OnDestroy()
    {
        // กันค่าค้างเป็น true ถ้าออกจากซีนกลางคันตอนกำลังนับถอยหลัง
        IsCountdownActive = false;
    }
}