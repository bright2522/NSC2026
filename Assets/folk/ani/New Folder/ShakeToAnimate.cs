using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ShakeToAnimate : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("ชื่อของ Trigger parameter ใน Animator")]
    [SerializeField] private string animationTriggerName = "PlayAnim";
    [Tooltip("จำนวนครั้งที่ต้องเขย่าให้ครบเพื่อปลดล็อกการเลื่อนจอ")]
    [SerializeField] private int requiredCountToUnlock = 3; // 🎯 เป้าหมายคือ 3 ครั้ง

    [Header("Shake Detection Settings")]
    [Tooltip("ความแรงในการเขย่า (ปรับให้เหมาะกับการลบ Gravity แล้ว ค่า 1.0 - 1.5 กำลังดี)")]
    [SerializeField] private float shakeThreshold = 1.2f;

    [Tooltip("ระยะเวลาหน่วงระหว่างการเขย่าหรือกดปุ่มแต่ละครั้ง (วินาที)")]
    [SerializeField] private float shakeCooldown = 1.0f;

    [Header("Swipe System Link")]
    [Tooltip("ลาก GameObject ที่มีสคริปต์ SwipeStationSlider มาใส่ตรงนี้")]
    [SerializeField] private SwipeStationSlider stationSlider;

    private Animator animator;
    private float lastShakeTime;
    private int currentAnimCount = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        
        // ดึง Instance อัตโนมัติถ้าไม่ได้ลากวางใน Inspector
        if (stationSlider == null) 
            stationSlider = SwipeStationSlider.Instance;

        // 🔒 สั่งล็อกการเลื่อนจอทันทีตอนเริ่มเกม!
        if (stationSlider != null)
        {
            stationSlider.SetSwipeEnabled(false);
            Debug.Log("<color=red>🔒 สั่งล็อกการเลื่อนจอตอนเริ่มเกม (ต้องเขย่า/กด W ให้ครบ 3 ครั้งก่อน)</color>");
        }
    }

    void Update()
    {
        // ถ้าเล่นครบเป้าหมายแล้ว ไม่ต้องตรวจจับเพิ่ม
        if (currentAnimCount >= requiredCountToUnlock) return;

        DetectShakeOrKey();
    }

    private void DetectShakeOrKey()
    {
        // ตรวจสอบ Cooldown
        if (Time.time < lastShakeTime + shakeCooldown) return;

        // 1. ตรวจจับการกดปุ่ม W (คีย์บอร์ด)
        bool isKeyPressed = Input.GetKeyDown(KeyCode.W);

        // 2. คำนวณความเร่งจากการเขย่า (โทรศัพท์)
        Vector3 acceleration = Input.acceleration;
        float currentAcceleration = acceleration.magnitude - 1.0f;
        bool isShaken = currentAcceleration >= shakeThreshold;

        // เงื่อนไข: ถ้ากดปุ่ม W หรือ เขย่าเครื่องแรงพอ
        if (isKeyPressed || isShaken)
        {
            TriggerAnimation();
        }
    }

    private void TriggerAnimation()
    {
        lastShakeTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger(animationTriggerName);
            currentAnimCount++;
            Debug.Log($"<color=cyan>เล่นอนิเมชั่นแล้ว {currentAnimCount}/{requiredCountToUnlock} ครั้ง</color>");

            // 🔓 เมื่อเล่นอนิเมชั่นครบตามเป้าหมาย (3 ครั้ง) แล้ว ถึงจะสั่งปลดล็อก!
            if (currentAnimCount >= requiredCountToUnlock)
            {
                if (stationSlider != null)
                {
                    stationSlider.SetSwipeEnabled(true);
                    Debug.Log("<color=green>🔓 เขย่า/กด W ครบ 3 ครั้งแล้ว! ปลดล็อกการปัดหน้าจอเรียบร้อย</color>");
                }
            }
        }
    }

    // ฟังก์ชันรีเซ็ตระบบเมื่อต้องการเริ่มนับใหม่ในซีนเดิม
    public void ResetShakeState()
    {
        currentAnimCount = 0;
        if (stationSlider != null)
        {
            stationSlider.SetSwipeEnabled(false);
        }
    }
}