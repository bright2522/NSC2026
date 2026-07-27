using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.SceneManagement; 
using System.Collections; // จำเป็นต้องใช้สำหรับระบบนับเวลาแบบ Coroutine

public class TimerSystem : MonoBehaviour
{
    [Header("--- UI Elements ---")]
    [SerializeField] private Image timerFillImage;     // ลาก TimerBar_Fill มาใส่ที่นี่
    [SerializeField] private GameObject resultPanel;   // ลาก Result_Panel มาใส่ที่นี่
    
    [Header("--- Text Display (Numbers Only) ---")]
    [SerializeField] private TextMeshProUGUI scoreText; // ลาก Score_Text มาใส่ 
    [SerializeField] private TextMeshProUGUI moneyText; // ลาก Money_Text มาใส่ 

    [Header("--- Animators ---")]
    [SerializeField] private Animator resultAnimator;  // ลาก Animator ของ Result_Panel มาใส่ช่องนี้

    [Header("--- Visual Effects ---")]
    [SerializeField] private ParticleSystem starParticles; // ลาก Particle ดาวมาใส่ช่องนี้

    [Header("--- Timer Settings ---")]
    [SerializeField] private float maxTime = 10f;      // ตั้งเวลาทำอาหารทั้งหมด (วินาที)
    private float currentTime;
    private bool isTimerRunning = true;

    [Header("--- Reward Settings (Fixed) ---")]
    [SerializeField] private int scoreReward = 100;    // คะแนนคงที่เมื่อจบเกม
    [SerializeField] private int moneyReward = 20;     // จำนวนเงินคงที่เมื่อจบเกม

    [Header("--- Scene Settings ---")]
    [SerializeField] private string gameOverSceneName = "pad"; // ชื่อซีนที่จะย้ายไป (ตรงกับใน Scene List)
    [SerializeField] private float delayAfterAnimation = 5.0f; // ระยะเวลารอก่อนย้ายซีน (5 วินาที)

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            if (currentTime > 0)
            {
                // ลดเวลาลงเรื่อยๆ ตามเวลาจริง
                currentTime -= Time.deltaTime;
                timerFillImage.fillAmount = currentTime / maxTime;
            }
            else
            {
                // เมื่อเวลาหมดหลอดพอดี
                currentTime = 0;
                timerFillImage.fillAmount = 0;
                isTimerRunning = false; // หยุดระบบเวลา

                TimeOut();
            }
        }
    }

    // ฟังก์ชันรีเซ็ตเวลา
    public void ResetTimer()
    {
        StopAllCoroutines(); // หยุดระบบนับเวลาถอยหลังทั้งหมดที่ค้างอยู่

        currentTime = maxTime;
        timerFillImage.fillAmount = 1f;
        isTimerRunning = true;

        if (resultPanel != null) resultPanel.SetActive(false);

        if (starParticles != null)
        {
            starParticles.Stop();
            starParticles.Clear();
        }
    }

    // ทำงานอัตโนมัติทันทีเมื่อเวลาหมดหลอด
    private void TimeOut()
    {
        Debug.Log("Time Out! หมดเวลาแล้ว");

        // 1. ใส่ค่าคะแนนและเงินคงที่ลงใน Text UI
        if (scoreText != null) scoreText.text = scoreReward.ToString();
        if (moneyText != null) moneyText.text = moneyReward.ToString();

        // 2. เปิดแสดง Result_Panel และสั่งเล่นอนิเมชั่นเด้งขึ้นมา
        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultAnimator != null)
        {
            resultAnimator.SetTrigger("Show");
        }

        // 3. เริ่มระบบลำดับเวลา (อนิเมชั่นจบ -> รอ 1 วิ -> ปล่อยดาว -> รอ 5 วิ -> เปลี่ยนซีน)
        StartCoroutine(SequenceAfterTimeOut());
    }

    private IEnumerator SequenceAfterTimeOut()
    {
        // ขั้นที่ 1: รอให้อนิเมชั่นเด้งขึ้นมาจนจบก่อน
        yield return new WaitForEndOfFrame();
        if (resultAnimator != null)
        {
            AnimatorStateInfo stateInfo = resultAnimator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length);
        }

        // ขั้นที่ 2: รอ 1 วินาทีตามที่กำหนด ก่อนปล่อยเอฟเฟกต์ดาว
        yield return new WaitForSeconds(0.5f);
        
        if (starParticles != null) 
        {
            starParticles.Play();
            Debug.Log("ปล่อยดาวกระจายเรียบร้อย เริ่มนับถอยหลัง 5 วินาที...");
        }

        // ขั้นที่ 3: รอต่ออีก 5 วินาทีเต็มๆ
        yield return new WaitForSeconds(delayAfterAnimation);

        // ขั้นที่ 4: เปลี่ยนซีนไปที่ "pad"
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        Debug.Log("กำลังเปลี่ยนไปยังซีน: " + gameOverSceneName);
        SceneManager.LoadScene(gameOverSceneName);
    }
}